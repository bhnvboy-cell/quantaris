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
                WriteJson(response, New With {.success = False, .result = "", .message = "Only POST method is allowed."})
                Return
            End If

            Dim body As String
            Using reader As New StreamReader(context.Request.InputStream, context.Request.ContentEncoding)
                body = reader.ReadToEnd()
            End Using

            Dim requestData = SimpleJsonDeserialize(body)
            If requestData Is Nothing OrElse Not requestData.ContainsKey("text") Then
                response.StatusCode = 400
                WriteJson(response, New With {.success = False, .result = "", .message = "Invalid request. 'text' field is required."})
                Return
            End If

            Dim text As String = requestData("text").ToString().Trim()
            Dim action As String = If(requestData.ContainsKey("action"), requestData("action").ToString().ToLower(), "grammar")
            Dim style As String = If(requestData.ContainsKey("style"), requestData("style").ToString().ToLower(), "professional")

            If String.IsNullOrWhiteSpace(text) Then
                WriteJson(response, New With {.success = True, .result = "", .issues = New List(Of Object)()})
                Return
            End If

            Dim issues As New List(Of Object)()

            ' Always run local grammar check
            issues.AddRange(RunLocalGrammarCheck(text))

            ' If action is grammar only (no API key needed), return now
            If action = "grammar" Then
                WriteJson(response, New With {.success = True, .result = text, .issues = issues})
                Return
            End If

            ' For rephrase/simplify, try OpenAI API
            Dim apiKey As String = ConfigurationManager.AppSettings("OpenAIKey")
            If String.IsNullOrWhiteSpace(apiKey) Then
                Dim msg = "OpenAI API key not configured. Set 'OpenAIKey' in web.config AppSettings."
                issues.Add(New With {.type = "info", .message = msg, .suggestion = ""})
                WriteJson(response, New With {.success = True, .result = text, .issues = issues})
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
                issues.Add(New With {.type = "error", .message = "OpenAI API call failed.", .suggestion = "Check API key and network connection."})
                WriteJson(response, New With {.success = True, .result = text, .issues = issues})
            Else
                ' Re-run local grammar check on the AI result
                Dim resultIssues = RunLocalGrammarCheck(apiResult)
                WriteJson(response, New With {.success = True, .result = apiResult, .issues = resultIssues})
            End If

        Catch ex As Exception
            response.StatusCode = 500
            WriteJson(response, New With {.success = False, .result = "", .message = "Server error: " & ex.Message})
        End Try
    End Sub

    Private Function RunLocalGrammarCheck(text As String) As List(Of Object)
        Dim issues As New List(Of Object)()

        ' Double spaces
        If Regex.IsMatch(text, "  +") Then
            issues.Add(New With {.type = "grammar", .message = "Double spaces found", .suggestion = "Remove extra spaces."})
        End If

        ' Missing period at end of sentence (non-empty, doesn't end with punctuation)
        Dim trimmed = text.TrimEnd()
        If trimmed.Length > 0 AndAlso Not trimmed.EndsWith(".") AndAlso Not trimmed.EndsWith("!") AndAlso Not trimmed.EndsWith("?") AndAlso Not trimmed.EndsWith(":") AndAlso Not trimmed.EndsWith(";") Then
            issues.Add(New With {.type = "grammar", .message = "Missing period at end of sentence", .suggestion = "Add a period at the end."})
        End If

        ' Sentence starting with lowercase
        Dim sentences = Regex.Split(text.Trim(), "(?<=[.!?])\s+")
        For Each sentence In sentences
            Dim s = sentence.Trim()
            If s.Length > 0 AndAlso Char.IsLower(s(0)) AndAlso Char.IsLetter(s(0)) Then
                issues.Add(New With {.type = "grammar", .message = "Sentence starts with lowercase: '" & Left(s, 30) & "...'", .suggestion = "Capitalize the first letter."})
            End If
        Next

        ' Common misspellings
        Dim misspellings As New Dictionary(Of String, String) From {
            {"recieve", "receive"},
            {"recieved", "received"},
            {"occured", "occurred"},
            {"occurence", "occurrence"},
            {"occurrance", "occurrence"},
            {"seperate", "separate"},
            {"seperated", "separated"},
            {"seperately", "separately"},
            {"acheive", "achieve"},
            {"acheived", "achieved"},
            {"accomodate", "accommodate"},
            {"accomodated", "accommodated"},
            {"neccessary", "necessary"},
            {"necesary", "necessary"},
            {"dependant", "dependent"},
            {"independant", "independent"},
            {"wierd", "weird"},
            {"definately", "definitely"},
            {"definatly", "definitely"},
            {"embarass", "embarrass"},
            {"enviroment", "environment"},
            {"existance", "existence"},
            {"goverment", "government"},
            {"maintainance", "maintenance"},
            {"millenium", "millennium"},
            {"occassion", "occasion"},
            {"priviledge", "privilege"},
            {"publically", "publicly"},
            {"refered", "referred"},
            {"seige", "siege"},
            {"supposably", "supposedly"},
            {"threshhold", "threshold"},
            {"tommorow", "tomorrow"},
            {"tounge", "tongue"},
            {"truely", "truly"},
            {"untill", "until"},
            {"withold", "withhold"},
            {"accomadation", "accommodation"},
            {"apparantly", "apparently"},
            {"calender", "calendar"},
            {"catagory", "category"},
            {"comming", "coming"},
            {"commitee", "committee"},
            {"completly", "completely"},
            {"consciencious", "conscientious"},
            {"consistant", "consistent"},
            {"decathalon", "decathlon"},
            {"dilema", "dilemma"},
            {"dissapear", "disappear"},
            {"dissapoint", "disappoint"},
            {"embarassment", "embarrassment"},
            {"equiptment", "equipment"},
            {"exersize", "exercise"},
            {"foriegn", "foreign"},
            {"freind", "friend"},
            {"fulsome", "fawning"},
            {"guage", "gauge"},
            {"harrass", "harass"},
            {"hygeine", "hygiene"},
            {"ignorence", "ignorance"},
            {"immediatly", "immediately"},
            {"incidently", "incidentally"},
            {"independance", "independence"},
            {"innoculate", "inoculate"},
            {"jeopardise", "jeopardize"},
            {"knowlege", "knowledge"},
            {"lesure", "leisure"},
            {"libary", "library"},
            {"liason", "liaison"},
            {"liason", "liaison"},
            {"manuever", "maneuver"},
            {"milage", "mileage"},
            {"miniscule", "minuscule"},
            {"mischievious", "mischievous"},
            {"neccessitate", "necessitate"},
            {"noticible", "noticeable"},
            {"occurrance", "occurrence"},
            {"persistant", "persistent"},
            {"personell", "personnel"},
            {"posession", "position"},
            {"privelege", "privilege"},
            {"probly", "probably"},
            {"pronounciation", "pronunciation"},
            {"realy", "really"},
            {"referance", "reference"},
            {"relevent", "relevant"},
            {"responsable", "responsible"},
            {"seize", "sieze"},
            {"succesful", "successful"},
            {"supercede", "supersede"},
            {"supress", "suppress"},
            {"temperment", "temperament"},
            {"therefor", "therefore"},
            {"transfered", "transferred"},
            {"truely", "truly"},
            {"unecessary", "unnecessary"},
            {"unfortunatly", "unfortunately"},
            {"untill", "until"},
            {"wether", "whether"}
        }

        Dim words = Regex.Split(text, "\W+")
        For Each word In words
            Dim lower = word.ToLower()
            If misspellings.ContainsKey(lower) Then
                issues.Add(New With {.type = "spelling", .message = "Misspelling: '" & word & "'", .suggestion = "Correct to: '" & misspellings(lower) & "'"})
            End If
        Next

        ' Passive voice detection (simple)
        Dim passivePattern = "\b(is|are|was|were|been|being|be)\s+(being\s+)?\w+ed\b"
        Dim passiveMatches = Regex.Matches(text, passivePattern, RegexOptions.IgnoreCase)
        If passiveMatches.Count > 0 Then
            For Each m As Match In passiveMatches
                If m.Index < 80 Then
                    issues.Add(New With {.type = "style", .message = "Possible passive voice: '" & m.Value & "'", .suggestion = "Consider rephrasing in active voice."})
                End If
            Next
        End If

        ' Missing articles before common nouns (simple heuristic)
        Dim articlePattern = "\b(task|process|system|method|approach|result|data|project|report|document|analysis|study|model|test|plan|solution|problem|issue|feature|function|module|component|service|method)\b"
        Dim articleMatches = Regex.Matches(text, articlePattern, RegexOptions.IgnoreCase)
        For Each m As Match In articleMatches
            If m.Index > 0 Then
                Dim precedingChar = text.Substring(m.Index - 1, 1)
                Dim precedingTwo = ""
                If m.Index >= 3 Then precedingTwo = text.Substring(m.Index - 3, 3).ToLower()
                If precedingChar <> " " OrElse
                   (Not precedingTwo.Contains("the") AndAlso
                    Not precedingTwo.Contains(" a") AndAlso
                    Not precedingTwo.Contains(" an") AndAlso
                    Not precedingTwo.Contains("its") AndAlso
                    Not precedingTwo.Contains("his") AndAlso
                    Not precedingTwo.Contains("her")) Then
                    ' Check word before the space
                    If m.Index >= 2 Then
                        Dim prevWord = Regex.Match(text.Substring(0, m.Index), "\w+$")
                        If prevWord.Success Then
                            Dim pw = prevWord.Value.ToLower()
                            If pw <> "the" AndAlso pw <> "a" AndAlso pw <> "an" AndAlso
                               pw <> "this" AndAlso pw <> "that" AndAlso pw <> "these" AndAlso pw <> "those" AndAlso
                               pw <> "my" AndAlso pw <> "your" AndAlso pw <> "our" AndAlso pw <> "their" AndAlso
                               pw <> "its" AndAlso pw <> "his" AndAlso pw <> "her" AndAlso
                               pw <> "each" AndAlso pw <> "every" AndAlso pw <> "some" AndAlso pw <> "any" AndAlso
                               pw <> "no" AndAlso pw <> "all" AndAlso pw <> "both" AndAlso
                               Not pw.EndsWith("'s") Then
                                issues.Add(New With {.type = "grammar", .message = "Missing article before '" & m.Value & "'", .suggestion = "Consider adding 'the', 'a', or 'an'."})
                            End If
                        End If
                    End If
                End If
            End If
        Next

        Return issues
    End Function

    Private Function CallOpenAI(apiKey As String, prompt As String, text As String) As String
        Try
            Dim url = "https://api.openai.com/v1/chat/completions"
            Dim requestBody = New With {
                .model = "gpt-3.5-turbo",
                .messages = New Object() {
                    New With {.role = "system", .content = "You are a helpful writing assistant. Follow instructions precisely."},
                    New With {.role = "user", .content = prompt & vbCrLf & vbCrLf & text}
                },
                .temperature = 0.7,
                .max_tokens = 2000
            }

            Dim jsonBody = SimpleJsonSerialize(requestBody)

            Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/json"
            request.Headers.Add("Authorization", "Bearer " & apiKey)
            request.Timeout = 30000

            Using stream = request.GetRequestStream()
                Dim bytes = Encoding.UTF8.GetBytes(jsonBody)
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

    ' Minimal JSON serializer
    Private Function SimpleJsonSerialize(obj As Object) As String
        If obj Is Nothing Then Return "null"

        If TypeOf obj Is String Then
            Return """" & EscapeJsonString(obj.ToString()) & """"
        ElseIf TypeOf obj Is Boolean Then
            Return If(CBool(obj), "true", "false")
        ElseIf TypeOf obj Is Integer OrElse TypeOf obj Is Long OrElse TypeOf obj Is Double OrElse TypeOf obj Is Decimal Then
            Return obj.ToString()
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
        Else
            ' Use anonymous type via reflection
            Dim props = obj.GetType().GetProperties()
            Dim dict As New Dictionary(Of String, Object)()
            For Each p In props
                dict(p.Name) = p.GetValue(obj)
            Next
            Return SimpleJsonSerialize(dict)
        End If
    End Function

    ' Minimal JSON deserializer (simple key-value and nested objects)
    Private Function SimpleJsonDeserialize(json As String) As Dictionary(Of String, Object)
        json = json.Trim()
        If Not json.StartsWith("{") Then Return Nothing

        Dim result As New Dictionary(Of String, Object)()
        Dim i = 1
        While i < json.Length
            ' Skip whitespace
            While i < json.Length AndAlso (json(i) = " "c OrElse json(i) = ControlChars.Cr OrElse json(i) = ControlChars.Lf OrElse json(i) = ControlChars.Tab)
                i += 1
            End While
            If i >= json.Length OrElse json(i) = "}"c Then Exit While

            ' Parse key
            Dim key = ParseJsonString(json, i)
            If key Is Nothing Then Exit While
            i = SkipWhitespace(json, i)
            If i < json.Length AndAlso json(i) = ":"c Then i += 1
            i = SkipWhitespace(json, i)

            ' Parse value
            Dim value As Object = Nothing
            Dim valueStr = ParseJsonValue(json, i)
            If valueStr IsNot Nothing Then
                value = valueStr.Value1
                i = valueStr.Value2
            End If

            result(key) = value

            i = SkipWhitespace(json, i)
            If i < json.Length AndAlso json(i) = ","c Then i += 1
        End While

        Return result
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

    Private Function ParseJsonValue(json As String, pos As Integer) As Tuple(Of Object, Integer)
        pos = SkipWhitespace(json, pos)
        If pos >= json.Length Then Return Nothing

        If json(pos) = """"c Then
            Dim s = ParseJsonString(json, pos)
            Return Tuple(Of Object, Integer)(s, pos)
        ElseIf json(pos) = "{"c Then
            Dim dict = SimpleJsonDeserialize(json.Substring(pos))
            ' Find matching end brace
            Dim depth = 0
            Dim j = pos
            While j < json.Length
                If json(j) = "{"c Then depth += 1
                If json(j) = "}"c Then
                    depth -= 1
                    If depth = 0 Then
                        j += 1
                        Exit While
                    End If
                End If
                j += 1
            End While
            Return Tuple(Of Object, Integer)(dict, j)
        ElseIf json(pos) = "["c Then
            Dim list As New List(Of Object)()
            Dim j = pos + 1
            While j < json.Length
                j = SkipWhitespace(json, j)
                If j >= json.Length OrElse json(j) = "]"c Then
                    j += 1
                    Exit While
                End If
                Dim itemVal = ParseJsonValue(json, j)
                If itemVal IsNot Nothing Then
                    list.Add(itemVal.Value1)
                    j = SkipWhitespace(json, itemVal.Value2)
                    If j < json.Length AndAlso json(j) = ","c Then j += 1
                Else
                    Exit While
                End If
            End While
            Return Tuple(Of Object, Integer)(list, j)
        Else
            ' Number, boolean, null
            Dim start = pos
            While pos < json.Length AndAlso json(pos) <> ","c AndAlso json(pos) <> "}"c AndAlso json(pos) <> "]"c AndAlso json(pos) <> " "c
                pos += 1
            End While
            Dim token = json.Substring(start, pos - start).ToLower()
            If token = "true" Then
                Return Tuple(Of Object, Integer)(True, pos)
            ElseIf token = "false" Then
                Return Tuple(Of Object, Integer)(False, pos)
            ElseIf token = "null" Then
                Return Tuple(Of Object, Integer)(Nothing, pos)
            Else
                Dim numVal As Double = 0
                If Double.TryParse(token, numVal) Then
                    If numVal = Math.Floor(numVal) AndAlso Math.Abs(numVal) < Integer.MaxValue Then
                        Return Tuple(Of Object, Integer)(CInt(numVal), pos)
                    End If
                    Return Tuple(Of Object, Integer)(numVal, pos)
                End If
                Return Tuple(Of Object, Integer)(token, pos)
            End If
        End If
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
