-- Quiz/Exam tables for Training LMS
-- Each course can have one or more quizzes
-- 2-attempt limit, 85% pass threshold, auto-certification

CREATE TABLE IF NOT EXISTS TrainingQuizzes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CourseId INT NOT NULL,
    QuizName VARCHAR(200) NOT NULL,
    PassingScore DOUBLE DEFAULT 85.0,
    MaxAttempts INT DEFAULT 2,
    TimeLimitMinutes INT,
    IsActive TINYINT(1) DEFAULT 1,
    CONSTRAINT fk_tq_course FOREIGN KEY (CourseId) REFERENCES TrainingCourses(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS QuizQuestions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuizId INT NOT NULL,
    QuestionText TEXT NOT NULL,
    OptionA VARCHAR(500) NOT NULL,
    OptionB VARCHAR(500) NOT NULL,
    OptionC VARCHAR(500) NOT NULL,
    OptionD VARCHAR(500) NOT NULL,
    CorrectAnswer CHAR(1) NOT NULL,
    QuestionOrder INT NOT NULL,
    CONSTRAINT fk_qq_quiz FOREIGN KEY (QuizId) REFERENCES TrainingQuizzes(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS QuizAttempts (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    QuizId INT NOT NULL,
    UserId INT NOT NULL,
    TenantId INT NOT NULL,
    AttemptNumber INT NOT NULL,
    Score DOUBLE NOT NULL,
    TotalQuestions INT NOT NULL,
    CorrectAnswers INT NOT NULL,
    StartedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CompletedOn DATETIME,
    Passed TINYINT(1) DEFAULT 0,
    CONSTRAINT fk_qa_quiz FOREIGN KEY (QuizId) REFERENCES TrainingQuizzes(Id) ON DELETE CASCADE,
    CONSTRAINT fk_qa_user FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT fk_qa_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
) ENGINE=InnoDB;

CREATE INDEX idx_quizattempts_user ON QuizAttempts(UserId, QuizId);

-- Seed: Six Sigma courses with ASQ-style quizzes
-- Only seeds if the DEMO tenant exists
SELECT @tid := Id FROM Tenants WHERE TenantCode = 'DEMO';

-- 1. White Belt - Six Sigma Foundations
INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours, IsActive)
VALUES ((SELECT @tid), 'WB-101', 'Six Sigma Foundations (White Belt)', 'White', 'Introduction to Six Sigma concepts, DMAIC overview, and quality basics.', 4, 1);
SET @cid := LAST_INSERT_ID();

INSERT INTO TrainingQuizzes (CourseId, QuizName, PassingScore, MaxAttempts) VALUES (@cid, 'White Belt Certification Exam', 85.0, 2);
SET @qid := LAST_INSERT_ID();

INSERT INTO QuizQuestions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder) VALUES
(@qid, 'What does DMAIC stand for?', 'Define, Measure, Analyze, Improve, Control', 'Design, Manufacture, Assemble, Inspect, Correct', 'Define, Monitor, Adjust, Implement, Check', 'Develop, Measure, Audit, Improve, Control', 'A', 1),
(@qid, 'What is the primary goal of Six Sigma?', 'Reduce defects to 3.4 per million opportunities', 'Increase production speed', 'Reduce employee turnover', 'Maximize profit', 'A', 2),
(@qid, 'Who is considered the father of Six Sigma?', 'W. Edwards Deming', 'Joseph Juran', 'Bill Smith', 'Philip Crosby', 'C', 3),
(@qid, 'What is a process in Six Sigma terms?', 'A sequence of steps that transforms inputs into outputs', 'A department in an organization', 'A software program', 'A quality checklist', 'A', 4),
(@qid, 'What is a defect in Six Sigma?', 'Any output that does not meet customer specifications', 'A broken machine', 'An employee error', 'A late delivery', 'A', 5),
(@qid, 'Which level of Six Sigma certification is entry-level?', 'White Belt', 'Black Belt', 'Green Belt', 'Master Black Belt', 'A', 6),
(@qid, 'What is CTQ in Six Sigma?', 'Critical to Quality', 'Cost to Quality', 'Critical to Quantity', 'Cycle Time Quality', 'A', 7),
(@qid, 'What does VOC stand for?', 'Voice of the Customer', 'Value of Control', 'Variation of Components', 'Verification of Compliance', 'A', 8),
(@qid, 'What is a process map used for?', 'To visually document the steps in a process', 'To calculate financial metrics', 'To assign project roles', 'To track defects', 'A', 9),
(@qid, 'What is a key principle of Six Sigma?', 'Data-driven decision making', 'Intuition-based management', 'Hierarchical decision making', 'Trial and error', 'A', 10);

-- 2. Yellow Belt - Process Improvement Fundamentals
INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours, IsActive)
VALUES ((SELECT @tid), 'YB-201', 'Process Improvement Fundamentals (Yellow Belt)', 'Yellow', 'Basic process improvement tools, data collection, and teamwork in Six Sigma projects.', 8, 1);
SET @cid := LAST_INSERT_ID();

INSERT INTO TrainingQuizzes (CourseId, QuizName, PassingScore, MaxAttempts) VALUES (@cid, 'Yellow Belt Certification Exam', 85.0, 2);
SET @qid := LAST_INSERT_ID();

INSERT INTO QuizQuestions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder) VALUES
(@qid, 'What is a SIPOC diagram?', 'A tool that maps Suppliers, Inputs, Process, Outputs, Customers', 'A financial reporting tool', 'A project scheduling tool', 'A defect tracking system', 'A', 1),
(@qid, 'Which role typically leads a Six Sigma project?', 'Black Belt', 'White Belt', 'Yellow Belt', 'Process Owner', 'A', 2),
(@qid, 'What is a key responsibility of a Yellow Belt?', 'Support project teams and participate in data collection', 'Lead enterprise-wide transformations', 'Develop corporate strategy', 'Manage budgets', 'A', 3),
(@qid, 'What tool is used to prioritize problems in Six Sigma?', 'Pareto Chart', 'Control Chart', 'Histogram', 'Scatter Plot', 'A', 4),
(@qid, 'What is a Pareto Chart based on?', 'The 80/20 rule', 'Normal distribution', 'Time series data', 'Correlation analysis', 'A', 5),
(@qid, 'What does a histogram show?', 'The distribution of data values', 'Trends over time', 'Cause and effect relationships', 'Process capability', 'A', 6),
(@qid, 'What is a check sheet used for?', 'Collecting and organizing data', 'Displaying statistical results', 'Planning project timelines', 'Assigning tasks', 'A', 7),
(@qid, 'What is the first step in DMAIC?', 'Define', 'Measure', 'Analyze', 'Improve', 'A', 8),
(@qid, 'What is a stakeholder in a Six Sigma project?', 'Anyone affected by the project', 'Only the project sponsor', 'Only the project team', 'External customers only', 'A', 9),
(@qid, 'What is a project charter?', 'A document that defines the project scope, goals, and team', 'A legal contract', 'A financial statement', 'A training manual', 'A', 10);

-- 3. Green Belt - Advanced Process Improvement
INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours, IsActive)
VALUES ((SELECT @tid), 'GB-301', 'Advanced Process Improvement (Green Belt)', 'Green', 'Statistical analysis tools, hypothesis testing, process capability, and FMEA.', 24, 1);
SET @cid := LAST_INSERT_ID();

INSERT INTO TrainingQuizzes (CourseId, QuizName, PassingScore, MaxAttempts) VALUES (@cid, 'Green Belt Certification Exam', 85.0, 2);
SET @qid := LAST_INSERT_ID();

INSERT INTO QuizQuestions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder) VALUES
(@qid, 'What is process capability (Cp)?', 'A measure of how well a process can meet specifications', 'The speed of a process', 'The cost of a process', 'The complexity of a process', 'A', 1),
(@qid, 'What does FMEA stand for?', 'Failure Mode and Effects Analysis', 'Failure Measurement and Error Analysis', 'Function Mode and Error Assessment', 'Financial Measurement and Evaluation', 'A', 2),
(@qid, 'What is the purpose of a Control Chart?', 'To monitor process variation over time', 'To show the distribution of data', 'To compare two processes', 'To calculate capability', 'A', 3),
(@qid, 'What is a p-value in hypothesis testing?', 'The probability of obtaining results at least as extreme as observed', 'The power of the test', 'The sample size', 'The confidence level', 'A', 4),
(@qid, 'What does RPN stand for in FMEA?', 'Risk Priority Number', 'Risk Prevention Number', 'Relative Performance Number', 'Reliability Parameter Number', 'A', 5),
(@qid, 'What is correlation?', 'A statistical measure of the relationship between two variables', 'A cause-and-effect relationship', 'A type of experimental design', 'A sampling method', 'A', 6),
(@qid, 'What is the Central Limit Theorem?', 'The distribution of sample means approaches normal as sample size increases', 'All data is normally distributed', 'The mean equals the median', 'Standard deviation is always 1', 'A', 7),
(@qid, 'What is A3 thinking?', 'A structured problem-solving approach using a single A3-sized page', 'A type of control chart', 'A sampling technique', 'A financial analysis method', 'A', 8),
(@qid, 'What is a Gage R&R study used for?', 'To assess the variation in a measurement system', 'To measure process capability', 'To analyze financial data', 'To design experiments', 'A', 9),
(@qid, 'What is the purpose of the Improve phase in DMAIC?', 'To develop and implement solutions', 'To define the problem', 'To measure current performance', 'To control the improved process', 'A', 10),
(@qid, 'What is a key output of the Measure phase?', 'Baseline data and process capability', 'The problem statement', 'The root cause analysis', 'The control plan', 'A', 11),
(@qid, 'What is hypothesis testing used for?', 'To make data-driven decisions about populations', 'To create process maps', 'To calculate project ROI', 'To assign team roles', 'A', 12),
(@qid, 'What does DOE stand for?', 'Design of Experiments', 'Data Observation Exercise', 'Direct Operational Evaluation', 'Development of Excellence', 'A', 13),
(@qid, 'What is a control plan?', 'A document describing how to maintain process improvements', 'A financial budget', 'A training schedule', 'A project timeline', 'A', 14),
(@qid, 'What is a mistake-proofing technique called?', 'Poka-Yoke', 'Kaizen', 'Kanban', 'Jidoka', 'A', 15);

-- 4. Black Belt - Expert Level
INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours, IsActive)
VALUES ((SELECT @tid), 'BB-401', 'Six Sigma Black Belt', 'Black', 'Advanced statistics, Design of Experiments, regression analysis, and lean enterprise.', 40, 1);
SET @cid := LAST_INSERT_ID();

INSERT INTO TrainingQuizzes (CourseId, QuizName, PassingScore, MaxAttempts) VALUES (@cid, 'Black Belt Certification Exam', 85.0, 2);
SET @qid := LAST_INSERT_ID();

INSERT INTO QuizQuestions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder) VALUES
(@qid, 'What is a fractional factorial design?', 'An experiment that uses only a subset of all possible combinations', 'A complete enumeration of all factor combinations', 'A single-factor experiment', 'A randomized block design', 'A', 1),
(@qid, 'What is a confounding in DOE?', 'When the effects of two or more factors cannot be separated', 'When the experiment is unbalanced', 'When data is missing', 'When factors are hard to change', 'A', 2),
(@qid, 'What is the difference between Cp and Cpk?', 'Cpk accounts for process centering, Cp does not', 'Cp is for short-term, Cpk is for long-term', 'Cp is for attributes, Cpk is for variables', 'There is no difference', 'A', 3),
(@qid, 'What is a response surface design?', 'A design used to optimize a response by fitting a polynomial model', 'A design for categorical factors', 'A screening design', 'A fractional factorial design', 'A', 4),
(@qid, 'What is a Box-Cox transformation?', 'A mathematical transformation to stabilize variance', 'A type of control chart', 'A sampling plan', 'A measurement system analysis', 'A', 5),
(@qid, 'What is the purpose of a residual plot?', 'To check the assumptions of a regression model', 'To show the distribution of data', 'To compare groups', 'To calculate confidence intervals', 'A', 6),
(@qid, 'What is a mixed model in DOE?', 'A model with both fixed and random effects', 'A model with only fixed effects', 'A model with only random effects', 'A non-linear model', 'A', 7),
(@qid, 'What is a power analysis used for?', 'To determine the sample size needed to detect an effect', 'To calculate the power of a machine', 'To analyze electrical systems', 'To measure energy consumption', 'A', 8),
(@qid, 'What is a Variance Inflation Factor (VIF)?', 'A measure of multicollinearity in regression', 'A measure of process variation', 'A measure of data dispersion', 'A measure of model fit', 'A', 9),
(@qid, 'What is the purpose of randomization in DOE?', 'To reduce bias by balancing unknown factors', 'To make the experiment faster', 'To reduce cost', 'To simplify analysis', 'A', 10),
(@qid, 'What is a main effect in a factorial design?', 'The effect of one factor averaged across levels of other factors', 'The largest effect in the experiment', 'The interaction between factors', 'The random error', 'A', 11),
(@qid, 'What is a center point in DOE?', 'An experimental run at the midpoint of all factor ranges', 'The average of all runs', 'The first run of an experiment', 'The last run of an experiment', 'A', 12),
(@qid, 'What is the purpose of a blocking variable?', 'To account for nuisance factors that may affect the response', 'To separate runs by time', 'To group similar treatments', 'To reduce the number of runs', 'A', 13),
(@qid, 'What is a normal probability plot used for?', 'To check if residuals are normally distributed', 'To show the distribution of raw data', 'To compare means', 'To calculate capability', 'A', 14),
(@qid, 'What is the purpose of Lean Six Sigma?', 'To combine waste reduction (Lean) with variation reduction (Six Sigma)', 'To focus only on speed', 'To focus only on quality', 'To reduce employee headcount', 'A', 15),
(@qid, 'What is a value stream map?', 'A tool that shows all steps in a process and identifies waste', 'A financial statement', 'A organizational chart', 'A project schedule', 'A', 16),
(@qid, 'What is takt time?', 'The rate at which products must be produced to meet customer demand', 'The time to complete one task', 'The total production time', 'The machine cycle time', 'A', 17),
(@qid, 'What is a Kaizen event?', 'A focused, short-term improvement project', 'A annual review', 'A financial audit', 'A training session', 'A', 18),
(@qid, 'What is 5S methodology?', 'Sort, Set in Order, Shine, Standardize, Sustain', 'Sort, Systematic, Shine, Standardize, Safety', 'Simplify, Set, Shine, Standardize, Sustain', 'Sort, Set, Shine, Simplify, Sustain', 'A', 19),
(@qid, 'What is a non-parametric test?', 'A test that does not assume a specific distribution', 'A test with no parameters', 'A test for parametric data', 'A test for proportions', 'A', 20);

-- 5. Master Black Belt - Leadership Level
INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours, IsActive)
VALUES ((SELECT @tid), 'MB-501', 'Master Black Belt', 'Master', 'Strategic deployment, mentoring, advanced analytics, and organizational transformation.', 48, 1);
SET @cid := LAST_INSERT_ID();

INSERT INTO TrainingQuizzes (CourseId, QuizName, PassingScore, MaxAttempts) VALUES (@cid, 'Master Black Belt Certification Exam', 85.0, 2);
SET @qid := LAST_INSERT_ID();

INSERT INTO QuizQuestions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder) VALUES
(@qid, 'What is Hoshin Kanri?', 'A strategic planning method for aligning goals across an organization', 'A quality control tool', 'A financial management system', 'A human resources process', 'A', 1),
(@qid, 'What is the role of a Master Black Belt?', 'To mentor Black Belts and drive strategic Six Sigma deployment', 'To lead individual projects', 'To manage daily operations', 'To handle customer complaints', 'A', 2),
(@qid, 'What is a tollgate review?', 'A phase review at the end of each DMAIC stage', 'A financial audit', 'A project closure meeting', 'A team building exercise', 'A', 3),
(@qid, 'What is the difference between a Master Black Belt and a Black Belt?', 'MBBs focus on strategy and mentoring; BBs lead projects', 'MBBs are higher in the organizational hierarchy', 'MBBs have more technical training', 'There is no difference', 'A', 4),
(@qid, 'What is a QFD matrix used for?', 'Translating customer requirements into product/process design', 'Measuring process capability', 'Designing experiments', 'Controlling process variation', 'A', 5),
(@qid, 'What is the primary focus of the Control phase in DMAIC?', 'Sustaining the gains through monitoring and standardization', 'Implementing solutions', 'Identifying root causes', 'Measuring baseline performance', 'A', 6),
(@qid, 'What is a governance structure in Six Sigma deployment?', 'The framework for overseeing and supporting projects', 'A financial reporting system', 'A project management tool', 'A performance review system', 'A', 7),
(@qid, 'What is a cost-benefit analysis used for in Six Sigma?', 'To evaluate the financial impact of improvement projects', 'To calculate process capability', 'To determine sample size', 'To assess measurement system', 'A', 8),
(@qid, 'What is change management in the context of Six Sigma?', 'The structured approach to transitioning individuals to a desired future state', 'A type of control chart', 'A project management methodology', 'A statistical tool', 'A', 9),
(@qid, 'What is the primary measure of success for a Six Sigma program?', 'Sustainable business results and cultural transformation', 'Number of projects completed', 'Number of Belts certified', 'Cost of the program', 'A', 10),
(@qid, 'What is AHP in decision making?', 'Analytic Hierarchy Process', 'Advanced Hypothesis Procedure', 'Automated Handling Process', 'Adaptive Human Protocol', 'A', 11),
(@qid, 'What is a balanced scorecard?', 'A strategic performance measurement framework', 'A financial statement', 'A project tracking tool', 'A quality dashboard', 'A', 12),
(@qid, 'What is the purpose of a maturity model?', 'To assess an organization''s Six Sigma capability level', 'To measure product quality', 'To evaluate employee performance', 'To calculate process capability', 'A', 13),
(@qid, 'What is design for Six Sigma (DFSS)?', 'A methodology for designing products and processes that meet customer needs', 'A variation of DMAIC', 'A cost reduction strategy', 'A training program', 'A', 14),
(@qid, 'What is a DMADV framework?', 'Define, Measure, Analyze, Design, Verify', 'Define, Measure, Analyze, Improve, Control', 'Design, Measure, Analyze, Develop, Verify', 'Define, Monitor, Assess, Design, Validate', 'A', 15);

-- 6. How to Operate Quantaris Software
INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours, IsActive)
VALUES ((SELECT @tid), 'QS-001', 'How to Operate Quantaris Software', 'White', 'Comprehensive guide to using the Quantaris Six Sigma platform - navigation, tools, data management, and best practices.', 2, 1);
SET @cid := LAST_INSERT_ID();

INSERT INTO TrainingQuizzes (CourseId, QuizName, PassingScore, MaxAttempts) VALUES (@cid, 'Quantaris Software Proficiency Exam', 85.0, 2);
SET @qid := LAST_INSERT_ID();

INSERT INTO QuizQuestions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectAnswer, QuestionOrder) VALUES
(@qid, 'What is the URL to access the Quantaris server?', 'http://localhost:8082', 'http://localhost:8080', 'http://quantaris.com', 'http://sixsigma.local', 'A', 1),
(@qid, 'Which database does Quantaris use?', 'PostgreSQL', 'SQL Server', 'MySQL', 'Oracle', 'A', 2),
(@qid, 'What is the default admin username?', 'admin', 'administrator', 'root', 'superuser', 'A', 3),
(@qid, 'Where can you find the FMEA tool in Quantaris?', 'In the Tools section of the sidebar', 'On the homepage only', 'In the Settings menu', 'In the Account page', 'A', 4),
(@qid, 'Which tool in Quantaris is used for Design of Experiments?', 'DOE under Tools', 'Regression Analysis', 'Process Capability', 'Control Charts', 'A', 5),
(@qid, 'How is tenant isolation implemented in Quantaris?', 'All tables have a TenantId column', 'Separate databases per tenant', 'Separate servers per tenant', 'Separate schemas per tenant', 'A', 6),
(@qid, 'What type of server does Quantaris use?', 'A standalone HTTP server with HttpListener', 'IIS', 'Apache', 'Nginx', 'A', 7),
(@qid, 'Which tool would you use to measure measurement system variation?', 'Gage R&R', 'Control Charts', 'Process Capability', 'Monte Carlo Simulation', 'A', 8),
(@qid, 'How do you run all database setup scripts?', 'Run SetupDatabase.bat', 'Run each SQL file manually', 'Use the Setup.aspx page', 'Restart the server', 'A', 9),
(@qid, 'What is the connection string name in Web.config?', 'QuantarisDb', 'DefaultConnection', 'QuantarisConn', 'DbConnection', 'A', 10);
