<%@ WebHandler Language="VB" Class="AIHandler" %>

Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web

Public Class AIHandler
    Implements IHttpHandler

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim response = context.Response
        response.ContentType = "application/json"
        response.ContentEncoding = Encoding.UTF8

        Try
            If context.Request.HttpMethod <> "POST" Then
                response.StatusCode = 405
                WriteJson(response, Dict("success", False, "result", "", "message", "Only POST method is allowed."))
                Return
            End If

            Dim body As String
            Using reader As New StreamReader(context.Request.InputStream, context.Request.ContentEncoding)
                body = reader.ReadToEnd()
            End Using

            Dim requestData = SimpleJsonDeserialize(body)
            If requestData Is Nothing OrElse Not requestData.ContainsKey("text") Then
                response.StatusCode = 400
                WriteJson(response, Dict("success", False, "result", "", "message", "Invalid request. 'text' field is required."))
                Return
            End If

            Dim text As String = requestData("text").ToString().Trim()
            Dim action As String = If(requestData.ContainsKey("action"), requestData("action").ToString().ToLower(), "grammar")
            Dim style As String = If(requestData.ContainsKey("style"), requestData("style").ToString().ToLower(), "professional")

            If String.IsNullOrWhiteSpace(text) Then
                WriteJson(response, Dict("success", True, "result", "", "issues", New List(Of Object)()))
                Return
            End If

            Dim issues As New List(Of Object)()
            issues.AddRange(RunLocalGrammarCheck(text))
            issues.AddRange(CallLanguageTool(text))

            If action = "grammar" Then
                Dim d As New Dictionary(Of String, Object)()
                d("success") = True
                d("result") = text
                d("issues") = issues
                WriteJson(response, d)
                Return
            End If

            ' Rephrase/Simplify: try OpenAI if key exists, otherwise return with grammar issues only
            Dim apiKey As String = GetStoredApiKey()
            If String.IsNullOrWhiteSpace(apiKey) Then
                Dim d As New Dictionary(Of String, Object)()
                d("success") = True
                d("result") = text
                d("issues") = issues
                WriteJson(response, d)
                Return
            End If

            Dim prompt As String = ""
            Select Case action
                Case "rephrase"
                    prompt = "Rephrase the following text in a " & style & " tone. Keep the same meaning but improve clarity and professionalism. Only return the rephrased text, nothing else."
                Case "simplify"
                    prompt = "Simplify the following text for clarity. Use shorter sentences and simpler words. Only return the simplified text, nothing else."
                Case Else
                    prompt = "Fix grammar and spelling errors in the following text. Only return the corrected text, nothing else."
            End Select

            Dim apiResult As String = CallOpenAI(apiKey, prompt, text)

            If apiResult Is Nothing Then
                issues.Add(Dict("type", "error", "message", "OpenAI API call failed.", "suggestion", "Check API key and network connection."))
                Dim d As New Dictionary(Of String, Object)()
                d("success") = True
                d("result") = text
                d("issues") = issues
                WriteJson(response, d)
            Else
                Dim resultIssues = RunLocalGrammarCheck(apiResult)
                Dim d As New Dictionary(Of String, Object)()
                d("success") = True
                d("result") = apiResult
                d("issues") = resultIssues
                WriteJson(response, d)
            End If

        Catch ex As Exception
            response.StatusCode = 500
            WriteJson(response, Dict("success", False, "result", "", "message", "Server error: " & ex.Message))
        End Try
    End Sub

    Private Function Dict(ParamArray items As Object()) As Dictionary(Of String, Object)
        Dim d As New Dictionary(Of String, Object)()
        Dim i = 0
        While i < items.Length - 1
            d(items(i).ToString()) = items(i + 1)
            i += 2
        End While
        Return d
    End Function

    Private Function GetStoredApiKey() As String
        Dim key = ConfigurationManager.AppSettings("OpenAIKey")
        If Not String.IsNullOrWhiteSpace(key) Then Return key
        Try
            Dim dataDir = AppDomain.CurrentDomain.GetData("DataDirectory").ToString()
            Dim keyFile = Path.Combine(dataDir, "openai_key.txt")
            If File.Exists(keyFile) Then
                key = File.ReadAllText(keyFile).Trim()
                If Not String.IsNullOrWhiteSpace(key) Then Return key
            End If
        Catch
        End Try
        Return ""
    End Function

    Private Function RunLocalGrammarCheck(text As String) As List(Of Object)
        Dim issues As New List(Of Object)()

        If Regex.IsMatch(text, "  +") Then
            issues.Add(Dict("type", "grammar", "message", "Double spaces found", "suggestion", "Remove extra spaces."))
        End If

        Dim trimmed = text.TrimEnd()
        If trimmed.Length > 0 AndAlso Not trimmed.EndsWith(".") AndAlso Not trimmed.EndsWith("!") AndAlso Not trimmed.EndsWith("?") AndAlso Not trimmed.EndsWith(":") AndAlso Not trimmed.EndsWith(";") Then
            issues.Add(Dict("type", "grammar", "message", "Missing period at end of sentence", "suggestion", "Add a period at the end."))
        End If

        Dim sentences = Regex.Split(text.Trim(), "(?<=[.!?])\s+")
        For Each sentence In sentences
            Dim s = sentence.Trim()
            If s.Length > 0 AndAlso Char.IsLower(s(0)) AndAlso Char.IsLetter(s(0)) Then
                issues.Add(Dict("type", "grammar", "message", "Sentence starts with lowercase: '" & Left(s, 30) & "...'", "suggestion", "Capitalize the first letter."))
            End If
        Next

        Dim misspellings As New Dictionary(Of String, String) From {
            {"recieve", "receive"}, {"recieved", "received"}, {"occured", "occurred"},
            {"occurence", "occurrence"}, {"occurrance", "occurrence"}, {"seperate", "separate"},
            {"seperated", "separated"}, {"seperately", "separately"}, {"acheive", "achieve"},
            {"acheived", "achieved"}, {"accomodate", "accommodate"}, {"accomodated", "accommodated"},
            {"neccessary", "necessary"}, {"necesary", "necessary"}, {"dependant", "dependent"},
            {"independant", "independent"}, {"wierd", "weird"}, {"definately", "definitely"},
            {"definatly", "definitely"}, {"embarass", "embarrass"}, {"enviroment", "environment"},
            {"existance", "existence"}, {"goverment", "government"}, {"maintainance", "maintenance"},
            {"millenium", "millennium"}, {"occassion", "occasion"}, {"priviledge", "privilege"},
            {"publically", "publicly"}, {"refered", "referred"}, {"seige", "siege"},
            {"supposably", "supposedly"}, {"threshhold", "threshold"}, {"tommorow", "tomorrow"},
            {"tounge", "tongue"}, {"truely", "truly"}, {"untill", "until"}, {"withold", "withhold"},
            {"accomadation", "accommodation"}, {"apparantly", "apparently"}, {"calender", "calendar"},
            {"catagory", "category"}, {"comming", "coming"}, {"commitee", "committee"},
            {"completly", "completely"}, {"consciencious", "conscientious"}, {"consistant", "consistent"},
            {"decathalon", "decathlon"}, {"dilema", "dilemma"}, {"dissapear", "disappear"},
            {"dissapoint", "disappoint"}, {"embarassment", "embarrassment"}, {"equiptment", "equipment"},
            {"exersize", "exercise"}, {"foriegn", "foreign"}, {"freind", "friend"},
            {"guage", "gauge"}, {"harrass", "harass"}, {"hygeine", "hygiene"},
            {"ignorence", "ignorance"}, {"immediatly", "immediately"},
            {"incidently", "incidentally"}, {"independance", "independence"}, {"innoculate", "inoculate"},
            {"jeopardise", "jeopardize"}, {"knowlege", "knowledge"}, {"lesure", "leisure"},
            {"libary", "library"}, {"liason", "liaison"}, {"manuever", "maneuver"},
            {"milage", "mileage"}, {"miniscule", "minuscule"}, {"mischievious", "mischievous"},
            {"neccessitate", "necessitate"}, {"noticible", "noticeable"},
            {"persistant", "persistent"}, {"personell", "personnel"}, {"posession", "position"},
            {"privelege", "privilege"}, {"probly", "probably"}, {"pronounciation", "pronunciation"},
            {"realy", "really"}, {"referance", "reference"}, {"relevent", "relevant"},
            {"responsable", "responsible"}, {"seize", "sieze"}, {"succesful", "successful"},
            {"supercede", "supersede"}, {"supress", "suppress"}, {"temperment", "temperament"},
            {"therefor", "therefore"}, {"transfered", "transferred"}, {"unecessary", "unnecessary"},
            {"unfortunatly", "unfortunately"}, {"wether", "whether"}
        }

        Dim words = Regex.Split(text, "\W+")
        For Each word In words
            Dim lower = word.ToLower()
            If misspellings.ContainsKey(lower) Then
                issues.Add(Dict("type", "spelling", "message", "Misspelling: '" & word & "'", "suggestion", "Correct to: '" & misspellings(lower) & "'"))
            End If
        Next

        Dim passivePattern = "\b(is|are|was|were|been|being|be)\s+(being\s+)?\w+ed\b"
        Dim passiveMatches = Regex.Matches(text, passivePattern, RegexOptions.IgnoreCase)
        If passiveMatches.Count > 0 Then
            For Each m As Match In passiveMatches
                If m.Index < 80 Then
                    issues.Add(Dict("type", "style", "message", "Possible passive voice: '" & m.Value & "'", "suggestion", "Consider rephrasing in active voice."))
                End If
            Next
        End If

        Return issues
    End Function

    Private Function CallLanguageTool(text As String) As List(Of Object)
        Dim issues As New List(Of Object)()
        Try
            Dim url = "https://api.languagetool.org/v2/check"
            Dim postData = "text=" & Uri.EscapeDataString(text) & "&language=en-US&enabledOnly=false"

            Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/x-www-form-urlencoded"
            request.Timeout = 10000

            Using stream = request.GetRequestStream()
                Dim bytes = Encoding.UTF8.GetBytes(postData)
                stream.Write(bytes, 0, bytes.Length)
            End Using

            Using webResponse = request.GetResponse()
                Using reader As New StreamReader(webResponse.GetResponseStream(), Encoding.UTF8)
                    Dim responseJson = reader.ReadToEnd()
                    Dim responseDict = SimpleJsonDeserialize(responseJson)
                    If responseDict IsNot Nothing AndAlso responseDict.ContainsKey("matches") Then
                        Dim matches = responseDict("matches")
                        If TypeOf matches Is List(Of Object) Then
                            For Each matchObj In CType(matches, List(Of Object))
                                If TypeOf matchObj Is Dictionary(Of String, Object) Then
                                    Dim m = CType(matchObj, Dictionary(Of String, Object))
                                    Dim msg = If(m.ContainsKey("message"), m("message").ToString(), "")
                                    Dim issueType = "grammar"
                                    Dim issueMsg = msg
                                    Dim suggestion = ""

                                    If m.ContainsKey("rule") AndAlso TypeOf m("rule") Is Dictionary(Of String, Object) Then
                                        Dim rule = CType(m("rule"), Dictionary(Of String, Object))
                                        If rule.ContainsKey("issueType") Then
                                            Dim it = rule("issueType").ToString().ToLower()
                                            If it = "misspelling" Then issueType = "spelling"
                                            If it = "style" Then issueType = "style"
                                            If it = "typographical" Then issueType = "grammar"
                                        End If
                                    End If

                                    If m.ContainsKey("replacements") AndAlso TypeOf m("replacements") Is List(Of Object) Then
                                        Dim reps = CType(m("replacements"), List(Of Object))
                                        If reps.Count > 0 AndAlso TypeOf reps(0) Is Dictionary(Of String, Object) Then
                                            suggestion = "Correct to: '" & CType(reps(0), Dictionary(Of String, Object))("value").ToString() & "'"
                                        End If
                                    End If

                                    issues.Add(Dict("type", issueType, "message", issueMsg, "suggestion", suggestion))
                                End If
                            Next
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            ' LanguageTool unavailable — local rules still apply
        End Try
        Return issues
    End Function

    Private Function CallOpenAI(apiKey As String, prompt As String, text As String) As String
        Try
            Dim url = "https://api.openai.com/v1/chat/completions"
            Dim requestBody = "{""model"":""gpt-3.5-turbo"",""messages"":[{""role"":""system"",""content"":""You are a helpful writing assistant.""},{""role"":""user"",""content"":" & EscapeJsonString(prompt & vbCrLf & vbCrLf & text) & "}],""temperature"":0.7,""max_tokens"":2000}"

            Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/json"
            request.Headers.Add("Authorization", "Bearer " & apiKey)
            request.Timeout = 30000

            Using stream = request.GetRequestStream()
                Dim bytes = Encoding.UTF8.GetBytes(requestBody)
                stream.Write(bytes, 0, bytes.Length)
            End Using

            Using webResponse = request.GetResponse()
                Using reader As New StreamReader(webResponse.GetResponseStream(), Encoding.UTF8)
                    Dim responseJson = reader.ReadToEnd()
                    Dim responseDict = SimpleJsonDeserialize(responseJson)
                    If responseDict IsNot Nothing AndAlso responseDict.ContainsKey("choices") Then
                        Dim choices = responseDict("choices")
                        If TypeOf choices Is List(Of Object) Then
                            Dim choiceList = CType(choices, List(Of Object))
                            If choiceList.Count > 0 Then
                                Dim firstChoice = choiceList(0)
                                If TypeOf firstChoice Is Dictionary(Of String, Object) Then
                                    Dim choiceDict = CType(firstChoice, Dictionary(Of String, Object))
                                    If choiceDict.ContainsKey("message") Then
                                        Dim msgObj = choiceDict("message")
                                        If TypeOf msgObj Is Dictionary(Of String, Object) Then
                                            Dim msgDict = CType(msgObj, Dictionary(Of String, Object))
                                            If msgDict.ContainsKey("content") Then
                                                Return msgDict("content").ToString().Trim()
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End Using
            End Using

            Return Nothing
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub WriteJson(response As HttpResponse, obj As Object)
        response.Write(SimpleJsonSerialize(obj))
    End Sub

    Private Function SimpleJsonSerialize(obj As Object) As String
        If obj Is Nothing Then Return "null"

        If TypeOf obj Is String Then
            Return """" & EscapeJsonString(obj.ToString()) & """"
        ElseIf TypeOf obj Is Boolean Then
            Return If(CBool(obj), "true", "false")
        ElseIf TypeOf obj Is Integer OrElse TypeOf obj Is Long OrElse TypeOf obj Is Short OrElse TypeOf obj Is Byte Then
            Return CStr(CInt(obj))
        ElseIf TypeOf obj Is Double OrElse TypeOf obj Is Single OrElse TypeOf obj Is Decimal Then
            Return CDbl(obj).ToString("R")
        ElseIf TypeOf obj Is Dictionary(Of String, Object) Then
            Dim sb As New StringBuilder("{")
            Dim first = True
            For Each kvp In CType(obj, Dictionary(Of String, Object))
                If Not first Then sb.Append(",")
                sb.Append("""" & EscapeJsonString(kvp.Key) & """:")
                sb.Append(SimpleJsonSerialize(kvp.Value))
                first = False
            Next
            sb.Append("}")
            Return sb.ToString()
        ElseIf TypeOf obj Is List(Of Object) Then
            Dim sb As New StringBuilder("[")
            Dim first = True
            For Each item In CType(obj, List(Of Object))
                If Not first Then sb.Append(",")
                sb.Append(SimpleJsonSerialize(item))
                first = False
            Next
            sb.Append("]")
            Return sb.ToString()
        ElseIf obj.GetType().IsArray Then
            Dim sb As New StringBuilder("[")
            Dim first = True
            For Each item In CType(obj, Array)
                If Not first Then sb.Append(",")
                sb.Append(SimpleJsonSerialize(item))
                first = False
            Next
            sb.Append("]")
            Return sb.ToString()
        Else
            Return """" & EscapeJsonString(obj.ToString()) & """"
        End If
    End Function

    Private Function SimpleJsonDeserialize(json As String) As Dictionary(Of String, Object)
        json = json.Trim()
        If Not json.StartsWith("{") Then Return Nothing

        Dim result As New Dictionary(Of String, Object)()
        Dim i = 1
        While i < json.Length
            While i < json.Length AndAlso (json(i) = " "c OrElse json(i) = ControlChars.Cr OrElse json(i) = ControlChars.Lf OrElse json(i) = ControlChars.Tab)
                i += 1
            End While
            If i >= json.Length OrElse json(i) = "}"c Then Exit While

            Dim key = ParseJsonString(json, i)
            If key Is Nothing Then Exit While
            i = SkipWhitespace(json, i)
            If i < json.Length AndAlso json(i) = ":"c Then i += 1
            i = SkipWhitespace(json, i)

            Dim nextPos As Integer = i
            Dim value As Object = ParseJsonToken(json, nextPos)
            i = nextPos

            If result.ContainsKey(key) Then
                result(key) = value
            Else
                result.Add(key, value)
            End If

            i = SkipWhitespace(json, i)
            If i < json.Length AndAlso json(i) = ","c Then i += 1
        End While

        Return result
    End Function

    Private Function ParseJsonToken(json As String, ByRef pos As Integer) As Object
        pos = SkipWhitespace(json, pos)
        If pos >= json.Length Then Return Nothing

        Dim c = json(pos)

        If c = """"c Then
            Return ParseJsonString(json, pos)
        ElseIf c = "{"c Then
            pos += 1
            Dim dict As New Dictionary(Of String, Object)()
            While pos < json.Length
                While pos < json.Length AndAlso (json(pos) = " "c OrElse json(pos) = ControlChars.Cr OrElse json(pos) = ControlChars.Lf OrElse json(pos) = ControlChars.Tab)
                    pos += 1
                End While
                If pos >= json.Length OrElse json(pos) = "}"c Then
                    pos += 1
                    Exit While
                End If
                Dim key = ParseJsonString(json, pos)
                If key Is Nothing Then Exit While
                pos = SkipWhitespace(json, pos)
                If pos < json.Length AndAlso json(pos) = ":"c Then pos += 1
                pos = SkipWhitespace(json, pos)
                Dim val = ParseJsonToken(json, pos)
                If dict.ContainsKey(key) Then
                    dict(key) = val
                Else
                    dict.Add(key, val)
                End If
                pos = SkipWhitespace(json, pos)
                If pos < json.Length AndAlso json(pos) = ","c Then pos += 1
            End While
            Return dict
        ElseIf c = "["c Then
            pos += 1
            Dim list As New List(Of Object)()
            While pos < json.Length
                pos = SkipWhitespace(json, pos)
                If pos >= json.Length OrElse json(pos) = "]"c Then
                    pos += 1
                    Exit While
                End If
                Dim item = ParseJsonToken(json, pos)
                list.Add(item)
                pos = SkipWhitespace(json, pos)
                If pos < json.Length AndAlso json(pos) = ","c Then pos += 1
            End While
            Return list
        ElseIf c = "t"c OrElse c = "f"c Then
            Dim start = pos
            While pos < json.Length AndAlso json(pos) <> ","c AndAlso json(pos) <> "}"c AndAlso json(pos) <> "]"c AndAlso json(pos) <> " "c
                pos += 1
            End While
            Dim token = json.Substring(start, pos - start).ToLower()
            Return token = "true"
        ElseIf c = "n"c Then
            Dim start = pos
            While pos < json.Length AndAlso json(pos) <> ","c AndAlso json(pos) <> "}"c AndAlso json(pos) <> "]"c AndAlso json(pos) <> " "c
                pos += 1
            End While
            Return Nothing
        ElseIf c = "-"c OrElse (c >= "0"c AndAlso c <= "9"c) Then
            Dim start = pos
            While pos < json.Length AndAlso json(pos) <> ","c AndAlso json(pos) <> "}"c AndAlso json(pos) <> "]"c AndAlso json(pos) <> " "c
                pos += 1
            End While
            Dim token = json.Substring(start, pos - start)
            Dim numVal As Double = 0
            If Double.TryParse(token, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, numVal) Then
                If numVal = Math.Floor(numVal) AndAlso Math.Abs(numVal) < 2147483647 Then
                    Return CInt(numVal)
                End If
                Return numVal
            End If
            Return token
        Else
            pos += 1
            Return Nothing
        End If
    End Function

    Private Function ParseJsonString(json As String, ByRef pos As Integer) As String
        pos = SkipWhitespace(json, pos)
        If pos >= json.Length OrElse json(pos) <> """"c Then Return Nothing
        pos += 1
        Dim start = pos
        While pos < json.Length
            If json(pos) = "\"c AndAlso pos + 1 < json.Length Then
                pos += 2
            ElseIf json(pos) = """"c Then
                Exit While
            Else
                pos += 1
            End If
        End While
        Dim result = json.Substring(start, pos - start)
        If pos < json.Length Then pos += 1
        Return result.Replace("\""", """").Replace("\\", "\").Replace("\n", vbLf).Replace("\r", vbCr).Replace("\t", vbTab)
    End Function

    Private Function SkipWhitespace(json As String, pos As Integer) As Integer
        While pos < json.Length AndAlso (json(pos) = " "c OrElse json(pos) = ControlChars.Cr OrElse json(pos) = ControlChars.Lf OrElse json(pos) = ControlChars.Tab)
            pos += 1
        End While
        Return pos
    End Function

    Private Function EscapeJsonString(s As String) As String
        If s Is Nothing Then Return ""
        Return s.Replace("\", "\\").Replace("""", "\""").Replace(vbLf, "\n").Replace(vbCr, "\r").Replace(vbTab, "\t")
    End Function
End Class
