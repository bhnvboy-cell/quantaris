<%@ Page Title="Training & Certification LMS" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="TrainingLMS.aspx.vb" Inherits="Tools_TrainingLMS" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .course-card { transition: all 0.2s; border-top: 3px solid; cursor: pointer; }
        .course-card:hover { transform: translateY(-2px); box-shadow: 0 4px 12px rgba(0,0,0,0.1); }
        .course-card.white-belt { border-top-color: #6c757d; }
        .course-card.yellow-belt { border-top-color: #ffc107; }
        .course-card.green-belt { border-top-color: #198754; }
        .course-card.black-belt { border-top-color: #212529; }
        .course-card.master-black-belt { border-top-color: #dc3545; }
        .cert-badge { width: 60px; height: 60px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 10px; text-align: center; line-height: 1.2; }
        .cert-badge.white { background: #e9ecef; color: #495057; }
        .cert-badge.yellow { background: #fff3cd; color: #856404; }
        .cert-badge.green { background: #d4edda; color: #155724; }
        .cert-badge.black { background: #212529; color: #fff; }
        .cert-badge.master { background: #dc3545; color: #fff; }
        .question-card { border-left: 4px solid #0d6efd; margin-bottom: 1rem; padding: 1rem; background: #f8f9fa; border-radius: 4px; }
        .question-card.correct { border-left-color: #198754; background: #d4edda; }
        .question-card.incorrect { border-left-color: #dc3545; background: #f8d7da; }
        .option-label { display: block; padding: 6px 12px; margin: 4px 0; border-radius: 4px; cursor: pointer; border: 1px solid #dee2e6; }
        .option-label:hover { background: #e9ecef; }
        .option-label.selected { background: #cfe2ff; border-color: #0d6efd; }
        .option-label.correct-answer { background: #d4edda; border-color: #198754; }
        .option-label.wrong-answer { background: #f8d7da; border-color: #dc3545; }
        .quiz-progress { height: 8px; border-radius: 4px; }
        .result-circle { width: 120px; height: 120px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-size: 2rem; font-weight: 700; margin: 0 auto; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-mortarboard me-2 text-primary"></i>Training &amp; Certification LMS</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active" id="breadcrumbLabel" runat="server">Training LMS</li>
                </ol>
            </nav>
        </div>
    </div>

    <asp:Panel ID="pnlLMS" runat="server">
        <!-- Dashboard Stats -->
        <div class="row g-3 mb-4">
            <div class="col-3">
                <div class="card border-0 bg-light py-2 px-3 text-center">
                    <div class="small text-muted">Available Courses</div>
                    <h3 class="mb-0"><asp:Literal ID="litCourseCount" runat="server" /></h3>
                </div>
            </div>
            <div class="col-3">
                <div class="card border-0 bg-light py-2 px-3 text-center">
                    <div class="small text-muted">Active Enrollments</div>
                    <h3 class="mb-0"><asp:Literal ID="litEnrollCount" runat="server" /></h3>
                </div>
            </div>
            <div class="col-3">
                <div class="card border-0 bg-light py-2 px-3 text-center">
                    <div class="small text-muted">Certifications</div>
                    <h3 class="mb-0"><asp:Literal ID="litCertCount" runat="server" /></h3>
                </div>
            </div>
            <div class="col-3">
                <div class="card border-0 bg-light py-2 px-3 text-center">
                    <div class="small text-muted">Completion Rate</div>
                    <h3 class="mb-0"><asp:Literal ID="litCompletionRate" runat="server" /></h3>
                </div>
            </div>
        </div>

        <ul class="nav nav-tabs mb-4" id="lmsTabs" role="tablist">
            <li class="nav-item" role="presentation">
                <button class="nav-link active" id="courses-tab" data-bs-toggle="tab" data-bs-target="#courses" type="button">Courses</button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="enrollments-tab" data-bs-toggle="tab" data-bs-target="#enrollments" type="button">My Progress</button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="certifications-tab" data-bs-toggle="tab" data-bs-target="#certifications" type="button">Certifications</button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="manage-tab" data-bs-toggle="tab" data-bs-target="#manage" type="button">Manage</button>
            </li>
        </ul>

        <div class="tab-content">
            <!-- Courses -->
            <div class="tab-pane fade show active" id="courses">
                <div class="row g-3">
                    <asp:Repeater ID="rptCourses" runat="server">
                        <ItemTemplate>
                            <div class="col-md-4">
                                <div class="card course-card <%# GetBeltClass(Eval("BeltLevel").ToString()) %> h-100">
                                    <div class="card-body d-flex flex-column">
                                        <div class="d-flex justify-content-between">
                                            <span class="badge bg-secondary"><%# Eval("CourseCode") %></span>
                                            <span class="badge <%# GetBeltBadge(Eval("BeltLevel").ToString()) %>"><%# Eval("BeltLevel") %></span>
                                        </div>
                                        <h6 class="mt-2 mb-1"><%# Eval("CourseName") %></h6>
                                        <div class="small text-muted mb-2 flex-grow-1"><%# Eval("Description") %></div>
                                        <div class="small mb-2"><i class="bi bi-clock me-1"></i><%# Eval("DurationHours") %> hrs</div>
                                        <div class="d-flex gap-2">
                                            <asp:LinkButton ID="btnTakeQuiz" runat="server" CssClass="btn btn-sm btn-outline-primary flex-fill"
                                                CommandName="TakeQuiz" CommandArgument='<%# Eval("Id") %>' OnCommand="CourseCommand">
                                                <i class="bi bi-pencil-square me-1"></i>Take Quiz
                                            </asp:LinkButton>
                                            <asp:HyperLink ID="btnCert" runat="server" CssClass="btn btn-sm btn-outline-success flex-fill"
                                                NavigateUrl='<%# "?view=cert&courseId=" & Eval("Id") & "&tenant=" & CurrentTenantCode %>'
                                                Visible='<%# HasCertification(Eval("Id")) %>'>
                                                <i class="bi bi-trophy me-1"></i>View Cert
                                            </asp:HyperLink>
                                        </div>
                                        <div class="mt-2">
                                            <asp:Literal ID="litQuizStatus" runat="server" Text='<%# GetQuizStatus(Eval("Id")) %>' />
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div id="divNoCourses" runat="server" class="col-12 text-center text-muted py-4" visible="false">No courses available.</div>
                </div>
            </div>

            <!-- My Progress (Enrollments + Quiz Attempts) -->
            <div class="tab-pane fade" id="enrollments">
                <div class="card shadow-sm">
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-hover align-middle mb-0 small">
                                <thead class="table-dark">
                                    <tr><th>Course</th><th>Quiz</th><th>Attempt</th><th>Score</th><th>Result</th><th>Date</th></tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptAttempts" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Eval("CourseName") %></td>
                                                <td><%# Eval("QuizName") %></td>
                                                <td><%# Eval("AttemptNumber") %> / 2</td>
                                                <td><%# Eval("Score") %>%</td>
                                                <td><span class="badge <%# IIf(If(IsDBNull(Eval("Passed")), False, CBool(Eval("Passed"))), "bg-success", "bg-danger") %>"><%# IIf(If(IsDBNull(Eval("Passed")), False, CBool(Eval("Passed"))), "PASS", "FAIL") %></span></td>
                                                <td><%# If(Eval("CompletedOn") IsNot DBNull.Value, CDate(Eval("CompletedOn")).ToString("MMM dd, yyyy HH:mm"), "-") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Certifications -->
            <div class="tab-pane fade" id="certifications">
                <div class="row g-3">
                    <asp:Repeater ID="rptCertifications" runat="server">
                        <ItemTemplate>
                            <div class="col-md-4">
                                <div class="card h-100">
                                    <div class="card-body text-center">
                                        <div class="cert-badge <%# GetBeltClass(Eval("BeltLevel").ToString()) %> mx-auto mb-2"><%# Eval("BeltLevel") %></div>
                                        <h6><%# Eval("CertificationName") %></h6>
                                        <div class="small text-muted"><%# Eval("UserName") %></div>
                                        <div class="small">Issued: <%# If(Eval("IssueDate") IsNot DBNull.Value, CDate(Eval("IssueDate")).ToString("MMM dd, yyyy"), "-") %></div>
                                        <span class="badge <%# IIf(If(IsDBNull(Eval("IsActive")), False, CBool(Eval("IsActive"))), "bg-success", "bg-secondary") %>"><%# IIf(If(IsDBNull(Eval("IsActive")), False, CBool(Eval("IsActive"))), "Active", "Expired") %></span>
                                    </div>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>

            <!-- Manage -->
            <div class="tab-pane fade" id="manage">
                <div class="row g-4">
                    <div class="col-md-6">
                        <div class="card shadow-sm">
                            <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-plus-circle me-2"></i>Add Course</h6></div>
                            <div class="card-body">
                                <asp:Literal ID="litMessage" runat="server" />
                                <div class="row g-2">
                                    <div class="col-4">
                                        <label class="form-label small fw-semibold">Code</label>
                                        <asp:TextBox ID="txtCourseCode" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. BB-101" />
                                    </div>
                                    <div class="col-8">
                                        <label class="form-label small fw-semibold">Name</label>
                                        <asp:TextBox ID="txtCourseName" runat="server" CssClass="form-control form-control-sm" />
                                    </div>
                                    <div class="col-4">
                                        <label class="form-label small fw-semibold">Belt Level</label>
                                        <asp:DropDownList ID="ddlBelt" runat="server" CssClass="form-select form-select-sm">
                                            <asp:ListItem Value="White">White Belt</asp:ListItem>
                                            <asp:ListItem Value="Yellow">Yellow Belt</asp:ListItem>
                                            <asp:ListItem Value="Green" Selected="True">Green Belt</asp:ListItem>
                                            <asp:ListItem Value="Black">Black Belt</asp:ListItem>
                                            <asp:ListItem Value="Master">Master Black Belt</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-4">
                                        <label class="form-label small fw-semibold">Duration (hrs)</label>
                                        <asp:TextBox ID="txtDuration" runat="server" CssClass="form-control form-control-sm" TextMode="Number" />
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label small fw-semibold">Description</label>
                                        <asp:TextBox ID="txtCourseDesc" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                                    </div>
                                    <div class="col-12">
                                        <asp:LinkButton ID="btnAddCourse" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnAddCourse_Click">
                                            <i class="bi bi-plus-lg me-1"></i>Add Course
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="card shadow-sm mt-3">
                            <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-question-circle me-2"></i>Manage Quiz Questions</h6></div>
                            <div class="card-body">
                                <div class="row g-2">
                                    <div class="col-6">
                                        <label class="form-label small fw-semibold">Course</label>
                                        <asp:DropDownList ID="ddlMgrCourse" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlMgrCourse_Changed" />
                                    </div>
                                    <div class="col-6">
                                        <label class="form-label small fw-semibold">Quiz</label>
                                        <asp:DropDownList ID="ddlMgrQuiz" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlMgrQuiz_Changed" />
                                    </div>
                                </div>
                                <asp:Panel ID="pnlQuestions" runat="server" CssClass="mt-3" Visible="false">
                                    <asp:Repeater ID="rptMgrQuestions" runat="server">
                                        <ItemTemplate>
                                            <div class="border rounded p-2 mb-2 small bg-light">
                                                <div class="fw-bold">Q<%# Eval("QuestionOrder") %>: <%# Eval("QuestionText") %></div>
                                                <div class="mt-1 ms-3">
                                                    <div class="<%# IIf(Eval("CorrectAnswer").ToString()="A","text-success fw-bold","") %>">A) <%# Eval("OptionA") %></div>
                                                    <div class="<%# IIf(Eval("CorrectAnswer").ToString()="B","text-success fw-bold","") %>">B) <%# Eval("OptionB") %></div>
                                                    <div class="<%# IIf(Eval("CorrectAnswer").ToString()="C","text-success fw-bold","") %>">C) <%# Eval("OptionC") %></div>
                                                    <div class="<%# IIf(Eval("CorrectAnswer").ToString()="D","text-success fw-bold","") %>">D) <%# Eval("OptionD") %></div>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </asp:Panel>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="card shadow-sm">
                            <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-trophy me-2"></i>Issue Certification</h6></div>
                            <div class="card-body">
                                <div class="row g-2">
                                    <div class="col-6">
                                        <label class="form-label small fw-semibold">User</label>
                                        <asp:DropDownList ID="ddlUser" runat="server" CssClass="form-select form-select-sm" />
                                    </div>
                                    <div class="col-6">
                                        <label class="form-label small fw-semibold">Certification Name</label>
                                        <asp:TextBox ID="txtCertName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Green Belt 2026" />
                                    </div>
                                    <div class="col-4">
                                        <label class="form-label small fw-semibold">Belt Level</label>
                                        <asp:DropDownList ID="ddlCertBelt" runat="server" CssClass="form-select form-select-sm">
                                            <asp:ListItem Value="White">White Belt</asp:ListItem>
                                            <asp:ListItem Value="Yellow">Yellow Belt</asp:ListItem>
                                            <asp:ListItem Value="Green">Green Belt</asp:ListItem>
                                            <asp:ListItem Value="Black">Black Belt</asp:ListItem>
                                            <asp:ListItem Value="Master">Master Black Belt</asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="col-4">
                                        <label class="form-label small fw-semibold">Issue Date</label>
                                        <asp:TextBox ID="txtIssueDate" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                                    </div>
                                    <div class="col-4">
                                        <label class="form-label small fw-semibold">Expiry</label>
                                        <asp:TextBox ID="txtExpiry" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                                    </div>
                                    <div class="col-12">
                                        <label class="form-label small fw-semibold">Issued By</label>
                                        <asp:TextBox ID="txtIssuedBy" runat="server" CssClass="form-control form-control-sm" />
                                    </div>
                                    <div class="col-12">
                                        <asp:LinkButton ID="btnIssueCert" runat="server" CssClass="btn btn-warning btn-sm" OnClick="btnIssueCert_Click">
                                            <i class="bi bi-trophy me-1"></i>Issue Certification
                                        </asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- Quiz Panel (shown when taking a quiz) -->
    <asp:Panel ID="pnlQuiz" runat="server" Visible="false">
        <div class="card shadow-sm">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i><asp:Literal ID="litQuizTitle" runat="server" /></h6>
                <span class="badge bg-light text-dark">Attempt <asp:Literal ID="litAttemptNum" runat="server" /> / 2</span>
            </div>
            <div class="card-body">
                <div class="progress quiz-progress mb-3">
                    <div id="progressBar" runat="server" class="progress-bar" role="progressbar" style="width: 0%"></div>
                </div>

                <asp:Panel ID="pnlQuizQuestions" runat="server" Visible="true">
                    <asp:Repeater ID="rptQuestions" runat="server">
                        <ItemTemplate>
                            <div class="question-card">
                                <div class="fw-bold mb-2">Question <%# Container.ItemIndex + 1 %>: <%# Eval("QuestionText") %></div>
                                <div class="ms-2">
                                    <label class="option-label">
                                        <input type="radio" name="q_<%# Eval("Id") %>" value="A" />
                                        A) <%# Eval("OptionA") %>
                                    </label>
                                    <label class="option-label">
                                        <input type="radio" name="q_<%# Eval("Id") %>" value="B" />
                                        B) <%# Eval("OptionB") %>
                                    </label>
                                    <label class="option-label">
                                        <input type="radio" name="q_<%# Eval("Id") %>" value="C" />
                                        C) <%# Eval("OptionC") %>
                                    </label>
                                    <label class="option-label">
                                        <input type="radio" name="q_<%# Eval("Id") %>" value="D" />
                                        D) <%# Eval("OptionD") %>
                                    </label>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    <div class="text-center mt-3">
                        <asp:LinkButton ID="btnSubmitQuiz" runat="server" CssClass="btn btn-primary px-5" OnClick="btnSubmitQuiz_Click">
                            <i class="bi bi-check-lg me-1"></i>Submit Quiz
                        </asp:LinkButton>
                    </div>
                </asp:Panel>

                <!-- Quiz Results -->
                <asp:Panel ID="pnlQuizResults" runat="server" Visible="false" CssClass="text-center">
                    <div class="result-circle <%# IIf(ViewState("QuizPassed") IsNot Nothing AndAlso CBool(ViewState("QuizPassed")), "bg-success text-white", "bg-danger text-white") %>">
                        <asp:Literal ID="litResultScore" runat="server" />
                    </div>
                    <h5 class="mt-3"><asp:Literal ID="litResultText" runat="server" /></h5>
                    <p class="text-muted">
                        Correct: <asp:Literal ID="litCorrectCount" runat="server" /> / <asp:Literal ID="litTotalCount" runat="server" />
                    </p>

                    <asp:Panel ID="pnlResultsReview" runat="server" CssClass="text-start mt-4">
                        <asp:Repeater ID="rptResults" runat="server">
                            <ItemTemplate>
                                <div class="question-card <%# IIf(Eval("IsCorrect"), "correct", "incorrect") %>">
                                    <div class="fw-bold mb-2">Question <%# Container.ItemIndex + 1 %>: <%# Eval("QuestionText") %></div>
                                    <div class="ms-2 small">
                                        <div class="<%# IIf(Eval("CorrectAnswer").ToString()="A","fw-bold","") %>">A) <%# Eval("OptionA") %> <%# IIf(Eval("CorrectAnswer").ToString()="A","<span class='text-success'>&#10003;</span>","") %></div>
                                        <div class="<%# IIf(Eval("CorrectAnswer").ToString()="B","fw-bold","") %>">B) <%# Eval("OptionB") %> <%# IIf(Eval("CorrectAnswer").ToString()="B","<span class='text-success'>&#10003;</span>","") %></div>
                                        <div class="<%# IIf(Eval("CorrectAnswer").ToString()="C","fw-bold","") %>">C) <%# Eval("OptionC") %> <%# IIf(Eval("CorrectAnswer").ToString()="C","<span class='text-success'>&#10003;</span>","") %></div>
                                        <div class="<%# IIf(Eval("CorrectAnswer").ToString()="D","fw-bold","") %>">D) <%# Eval("OptionD") %> <%# IIf(Eval("CorrectAnswer").ToString()="D","<span class='text-success'>&#10003;</span>","") %></div>
                                        <div class="mt-1"><%# IIf(Eval("UserAnswer").ToString() = "", "<em>Not answered</em>", "Your answer: " & Eval("UserAnswer")) %></div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>

                    <div class="mt-4">
                        <asp:LinkButton ID="btnRetryQuiz" runat="server" CssClass="btn btn-warning me-2" OnClick="btnRetryQuiz_Click" Visible="false">
                            <i class="bi bi-arrow-counterclockwise me-1"></i>Retry Quiz
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnBackToCourses" runat="server" CssClass="btn btn-outline-secondary" OnClick="btnBackToCourses_Click">
                            <i class="bi bi-arrow-left me-1"></i>Back to Courses
                        </asp:LinkButton>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </asp:Panel>
</asp:Content>
