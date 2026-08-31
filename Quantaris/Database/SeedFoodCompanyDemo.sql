-- ============================================================
-- Food Company Six Sigma Demo Projects (MySQL)
-- 3 projects: Pizza Crust, Hot Sauce Fill, Packaging Seal
-- Requires the DEMO tenant (created by CreateMySQL.sql) and
-- admin user (created by CreateMySQL.sql).
-- ============================================================

SET @dt := CURDATE();
SELECT @tid := Id FROM Tenants WHERE TenantCode = 'DEMO';
SELECT @uid := Id FROM Users WHERE Username = 'admin' AND TenantId = @tid;
SET @uid := IFNULL(@uid, 1);

-- ============================================================
-- PROJECT 1: Pizza Crust Thickness Reduction
-- Problem: Crust thickness varies 8-18mm (target 12mm +/- 2mm)
-- ============================================================
INSERT INTO QuantarisProjects (TenantId, ProjectCode, ProjectName, Description, Status, Phase, Priority, FinancialImpact, StartDate, TargetEndDate, ProjectOwnerId, Champion, CreatedByUserId)
VALUES (@tid, 'FPC-2026-001', 'Pizza Crust Thickness Variation Reduction',
        'Reducing crust thickness variation from 8-18mm range to 10-14mm (target 12mm +/- 2mm). Current sigma level ~1.8.',
        'InProgress', 'Analyze', 'High', 185000.00, DATE_SUB(@dt, INTERVAL 45 DAY), DATE_ADD(@dt, INTERVAL 45 DAY), @uid, 'Maria Rossi - Plant Manager', @uid);
SET @pid1 := LAST_INSERT_ID();

INSERT INTO ProjectCharters (TenantId, ProjectId, ProblemStatement, GoalStatement, BusinessCase, ScopeIncluded, ScopeExcluded, Timeline, ElevatorSpeech, CreatedByUserId)
VALUES (@tid, @pid1,
        'Customer complaints about inconsistent pizza crust thickness have risen 40% in Q1. Crust thickness ranges from 8mm to 18mm, with a target of 12mm +/- 2mm.',
        'Reduce crust thickness variation to 12mm +/- 2mm (Cp >= 1.33, Cpk >= 1.33) within 90 days, reducing customer complaints by 80%.',
        'Each complaint costs $45 in customer service + $12 in replacement. Estimated annual savings of $185,000 from reduced waste and complaint handling.',
        'Dough mixing through baking and cooling. All three shifts at the Chicago plant.',
        'Raw material sourcing, supplier quality, packaging, and distribution.',
        'Define: Week 1-2 | Measure: Week 3-5 | Analyze: Week 6-8 | Improve: Week 9-11 | Control: Week 12-13',
        'Our pizza crusts are inconsistent. We will use Six Sigma to find root causes and make every crust perfect, saving $185K/year.',
        @uid);

-- VOC for Project 1
INSERT INTO VOC_Records (TenantId, ProjectId, CustomerName, VoiceStatement, CreatedByUserId)
VALUES
(@tid, @pid1, 'Pizza Hut Midwest', 'Crust thickness is inconsistent - some pizzas have thin burnt crusts, others thick doughy ones.', @uid),
(@tid, @pid1, 'Dominos Chicago', 'We are getting variable bake times because crusts are different thicknesses.', @uid),
(@tid, @pid1, 'Little Caesars', 'The last shipment had crusts ranging from 8mm to 17mm. This is unacceptable.', @uid);

-- COPQ for Project 1
INSERT INTO COPQ (TenantId, ProjectId, EntryDate, AppraisalCosts, PreventionCosts, InternalFailureCosts, ExternalFailureCosts, Revenue, Notes, CreatedByUserId)
VALUES
(@tid, @pid1, DATE_SUB(@dt, INTERVAL 45 DAY), 2500.00, 1800.00, 8500.00, 12300.00, 450000.00, 'Baseline COPQ - Q1 averages', @uid),
(@tid, @pid1, DATE_SUB(@dt, INTERVAL 30 DAY), 2100.00, 2200.00, 7200.00, 9800.00, 460000.00, 'Week 2 - after initial measurement', @uid),
(@tid, @pid1, DATE_SUB(@dt, INTERVAL 15 DAY), 1900.00, 3500.00, 5800.00, 7600.00, 455000.00, 'Week 4 - after FMEA implementation', @uid);

-- Process Capability (Baseline)
INSERT INTO ProcessCapability (TenantId, ProjectId, MetricName, USL, LSL, Target, Mean, StdDev, Cp, Cpk, Ppk, SigmaLevel, DPMO, SampleSize, DataPoints, CreatedByUserId)
VALUES (@tid, @pid1, 'Crust Thickness (mm)', 14.0, 10.0, 12.0, 12.8, 1.6, 0.42, 0.25, 0.22, 1.8, 158000.0, 100,
        '10.2,11.5,9.8,13.2,14.5,11.8,12.2,15.1,10.8,13.9,11.1,12.7,14.2,9.5,13.8,11.4,12.9,10.5,14.8,11.2,13.5,12.1,10.3,15.5,11.9,12.6,9.2,14.1,13.3,10.7,12.4,11.6,14.6,10.1,13.1,12.8,11.3,15.2,10.6,12.3,13.7,11.0,14.3,9.6,12.5,13.4,10.9,14.7,11.7,12.0,13.6,10.4,14.9,11.2,12.8,9.4,14.4,13.9,10.8,12.1,15.3,11.5,13.2,10.2,12.7,14.0,9.7,13.8,12.4,11.1,14.5,10.5,13.1,12.9,11.3,15.0,10.7,12.6,14.2,11.8,13.4,10.1,12.3,14.8,11.0,13.5,9.9,14.1,12.2,10.4,13.7,12.5,11.6,14.6,10.3,13.0,12.8,11.4,15.4,10.9',
        @uid);

-- FMEA for Project 1
INSERT INTO FMEA_Records (TenantId, ProjectId, ProcessStep, FailureMode, EffectOfFailure, CauseOfFailure, CurrentControl, Severity, Occurrence, Detection, RecommendedAction, CreatedByUserId)
VALUES
(@tid, @pid1, 'Dough Mixing', 'Inconsistent dough hydration', 'Variable dough consistency leads to thickness variation', 'Water temperature not controlled (+/- 5C range)', 'Operator checks temp manually once per hour', 7, 6, 5, 'Install automated water temperature control system', @uid),
(@tid, @pid1, 'Dough Dividing', 'Weight variation in dough balls', 'Different ball sizes produce different crust thicknesses', 'Divider machine not calibrated regularly', 'Weekly calibration check', 8, 5, 4, 'Implement daily calibration with SPC chart', @uid),
(@tid, @pid1, 'Sheeting', 'Inconsistent roller gap', 'Directly causes thickness variation in crusts', 'Roller wear and thermal expansion not compensated', 'Monthly maintenance check', 9, 7, 6, 'Install auto-adjustment with thickness feedback loop', @uid),
(@tid, @pid1, 'Proofing', 'Uneven proofing temperature', 'Some dough proofs faster, causing thickness differences', 'Proofer has hot/cold spots (temp variance +/- 3C)', 'None - no monitoring system', 6, 7, 7, 'Install thermal mapping and rebalance airflow', @uid),
(@tid, @pid1, 'Baking', 'Oven temperature variation', 'Inconsistent bake causes thickness and texture variation', 'Belt speed and zone temperature drift', 'Daily temperature log', 7, 5, 4, 'Install continuous oven profiling system', @uid);

-- Gantt Tasks
INSERT INTO GanttTasks (TenantId, ProjectId, TaskName, Phase, StartDate, EndDate, Progress, CreatedByUserId)
VALUES
(@tid, @pid1, 'Project Charter Approval', 'Define', DATE_SUB(@dt, INTERVAL 45 DAY), DATE_SUB(@dt, INTERVAL 40 DAY), 100, @uid),
(@tid, @pid1, 'SIPOC Mapping', 'Define', DATE_SUB(@dt, INTERVAL 40 DAY), DATE_SUB(@dt, INTERVAL 35 DAY), 100, @uid),
(@tid, @pid1, 'Gage R&R Study', 'Measure', DATE_SUB(@dt, INTERVAL 30 DAY), DATE_SUB(@dt, INTERVAL 25 DAY), 100, @uid),
(@tid, @pid1, 'Baseline Data Collection', 'Measure', DATE_SUB(@dt, INTERVAL 25 DAY), DATE_SUB(@dt, INTERVAL 20 DAY), 100, @uid),
(@tid, @pid1, 'Fishbone Analysis', 'Analyze', DATE_SUB(@dt, INTERVAL 18 DAY), DATE_SUB(@dt, INTERVAL 15 DAY), 100, @uid),
(@tid, @pid1, 'FMEA Workshop', 'Analyze', DATE_SUB(@dt, INTERVAL 15 DAY), DATE_SUB(@dt, INTERVAL 12 DAY), 100, @uid),
(@tid, @pid1, 'Hypothesis Testing', 'Analyze', DATE_SUB(@dt, INTERVAL 10 DAY), DATE_SUB(@dt, INTERVAL 5 DAY), 40, @uid),
(@tid, @pid1, 'Solution Design', 'Improve', DATE_ADD(@dt, INTERVAL 10 DAY), DATE_ADD(@dt, INTERVAL 18 DAY), 0, @uid),
(@tid, @pid1, 'Pilot Run', 'Improve', DATE_ADD(@dt, INTERVAL 20 DAY), DATE_ADD(@dt, INTERVAL 28 DAY), 0, @uid),
(@tid, @pid1, 'Control Plan Implementation', 'Control', DATE_ADD(@dt, INTERVAL 35 DAY), DATE_ADD(@dt, INTERVAL 42 DAY), 0, @uid);

INSERT INTO GanttMilestones (TenantId, ProjectId, MilestoneName, MilestoneDate, CreatedByUserId)
VALUES
(@tid, @pid1, 'Charter Signed', DATE_SUB(@dt, INTERVAL 40 DAY), @uid),
(@tid, @pid1, 'Baseline Data Complete', DATE_SUB(@dt, INTERVAL 20 DAY), @uid),
(@tid, @pid1, 'Root Causes Confirmed', DATE_SUB(@dt, INTERVAL 5 DAY), @uid),
(@tid, @pid1, 'Pilot Complete', DATE_ADD(@dt, INTERVAL 28 DAY), @uid),
(@tid, @pid1, 'Project Closure', DATE_ADD(@dt, INTERVAL 45 DAY), @uid);

-- Root Cause Analysis for Project 1
INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, Effect, IsFishbone, CreatedByUserId)
VALUES
(@tid, @pid1, 'Methods', 'No standard operating procedure for thickness adjustment', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Methods', 'Operator-dependent roller gap settings', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Machines', 'Roller gap drifts due to thermal expansion', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Machines', 'Proofer has uneven temperature distribution', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Materials', 'Flour protein content varies by supplier batch', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Materials', 'Water temperature not controlled', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Measurement', 'Thickness gauge not calibrated daily', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Measurement', 'Operators measure at inconsistent locations', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Environment', 'Ambient temperature varies between shifts', 'Crust thickness variation', true, @uid),
(@tid, @pid1, 'Environment', 'Humidity affects dough consistency', 'Crust thickness variation', true, @uid);

-- DMAIC Steps
INSERT INTO DMAICSteps (TenantId, ProjectId, Phase, StepNumber, StepName, Status, TargetDate, CreatedByUserId)
VALUES
(@tid, @pid1, 'Define', 1, 'Identify Problem & Create Charter', 'Completed', DATE_SUB(@dt, INTERVAL 45 DAY), @uid),
(@tid, @pid1, 'Define', 2, 'Define Scope & Team', 'Completed', DATE_SUB(@dt, INTERVAL 42 DAY), @uid),
(@tid, @pid1, 'Define', 3, 'Collect VOC & CTQ', 'Completed', DATE_SUB(@dt, INTERVAL 38 DAY), @uid),
(@tid, @pid1, 'Measure', 4, 'Map Process (SIPOC)', 'Completed', DATE_SUB(@dt, INTERVAL 35 DAY), @uid),
(@tid, @pid1, 'Measure', 5, 'Define Measurement System', 'Completed', DATE_SUB(@dt, INTERVAL 30 DAY), @uid),
(@tid, @pid1, 'Measure', 6, 'Conduct Gage R&R', 'Completed', DATE_SUB(@dt, INTERVAL 28 DAY), @uid),
(@tid, @pid1, 'Measure', 7, 'Collect Baseline Data', 'Completed', DATE_SUB(@dt, INTERVAL 25 DAY), @uid),
(@tid, @pid1, 'Measure', 8, 'Establish Process Capability', 'Completed', DATE_SUB(@dt, INTERVAL 22 DAY), @uid),
(@tid, @pid1, 'Analyze', 9, 'Identify Root Causes (Fishbone)', 'Completed', DATE_SUB(@dt, INTERVAL 18 DAY), @uid),
(@tid, @pid1, 'Analyze', 10, 'FMEA Analysis', 'Completed', DATE_SUB(@dt, INTERVAL 15 DAY), @uid),
(@tid, @pid1, 'Analyze', 11, 'Data Analysis & Hypothesis Testing', 'InProgress', DATE_SUB(@dt, INTERVAL 10 DAY), @uid),
(@tid, @pid1, 'Analyze', 12, 'Confirm Root Causes', 'NotStarted', DATE_SUB(@dt, INTERVAL 5 DAY), @uid),
(@tid, @pid1, 'Improve', 13, 'Generate Solutions', 'NotStarted', DATE_ADD(@dt, INTERVAL 10 DAY), @uid),
(@tid, @pid1, 'Improve', 14, 'Pilot Implementation', 'NotStarted', DATE_ADD(@dt, INTERVAL 20 DAY), @uid),
(@tid, @pid1, 'Improve', 15, 'Evaluate Results', 'NotStarted', DATE_ADD(@dt, INTERVAL 30 DAY), @uid),
(@tid, @pid1, 'Control', 13, 'Institutionalize the Solutions', 'NotStarted', DATE_ADD(@dt, INTERVAL 35 DAY), @uid),
(@tid, @pid1, 'Control', 14, 'Replicate and Share Best Practices', 'NotStarted', DATE_ADD(@dt, INTERVAL 42 DAY), @uid),
(@tid, @pid1, 'Control', 15, 'Celebrate and Recognize Success', 'NotStarted', DATE_ADD(@dt, INTERVAL 45 DAY), @uid);

-- ============================================================
-- PROJECT 2: Hot Sauce Fill Weight Accuracy
-- Problem: Fill weights vary 240-270ml (target 250ml +/- 5ml)
-- ============================================================
INSERT INTO QuantarisProjects (TenantId, ProjectCode, ProjectName, Description, Status, Phase, Priority, FinancialImpact, StartDate, TargetEndDate, ProjectOwnerId, Champion, CreatedByUserId)
VALUES (@tid, 'FPC-2026-002', 'Hot Sauce Fill Weight Accuracy Improvement',
        'Improve hot sauce bottle fill weight accuracy from current range (240-270ml) to 250ml +/- 5ml, reducing giveaway and underfill risks. Current sigma level ~2.1.',
        'InProgress', 'Analyze', 'High', 220000.00, DATE_SUB(@dt, INTERVAL 50 DAY), DATE_ADD(@dt, INTERVAL 40 DAY), @uid, 'Carlos Mendez - Production Director', @uid);
SET @pid2 := LAST_INSERT_ID();

INSERT INTO ProjectCharters (TenantId, ProjectId, ProblemStatement, GoalStatement, BusinessCase, ScopeIncluded, ScopeExcluded, Timeline, ElevatorSpeech, CreatedByUserId)
VALUES (@tid, @pid2,
        'Hot sauce fill weights are overfilling by average 8ml per bottle (250ml target). Annual overfill cost: $180,000. 1.2% of bottles are underfilled (<245ml), risking FDA compliance.',
        'Reduce fill weight variation to 250ml +/- 3ml (Cp >= 1.5) within 80 days, eliminating underfills and reducing average overfill to 1ml.',
        'Reducing overfill by 7ml saves $157,000/year in product giveaway. Eliminating underfills saves $63,000/year in compliance risk.',
        'All 3 production lines, both shifts. Filling through capping and checkweighing.',
        'Raw material receiving, labeling, packaging, warehousing.',
        'Define: Week 1-2 | Measure: Week 3-5 | Analyze: Week 6-7 | Improve: Week 8-10 | Control: Week 11-12',
        'We are giving away $180K/year in extra hot sauce. Six Sigma will dial in our fillers to save money and stay compliant.',
        @uid);

-- FMEA for Project 2
INSERT INTO FMEA_Records (TenantId, ProjectId, ProcessStep, FailureMode, EffectOfFailure, CauseOfFailure, CurrentControl, Severity, Occurrence, Detection, RecommendedAction, CreatedByUserId)
VALUES
(@tid, @pid2, 'Filling', 'Fill nozzle drip after cycle', 'Overfill by 2-5ml per bottle', 'Worn nozzle seals on 3 of 12 nozzles', 'Visual inspection every 2 hours', 6, 7, 5, 'Replace all nozzle seals monthly with preventive maintenance', @uid),
(@tid, @pid2, 'Filling', 'Pump pressure fluctuation', 'Inconsistent fill volumes across bottles', 'Air in hydraulic lines, pump wear', 'Pressure gauge monitoring (manual)', 7, 6, 6, 'Install automated pressure regulation with alarms', @uid),
(@tid, @pid2, 'Filling', 'Temperature variation in product', 'Product density changes affect fill volume', 'Hot sauce cooled unevenly in holding tank', 'None - no temperature control in filler', 5, 6, 7, 'Install temperature control jacket on holding tank', @uid),
(@tid, @pid2, 'Checkweighing', 'Scale drift', 'False reject or accept of bottles', 'Scale not calibrated, temperature effects', 'Weekly calibration check', 8, 4, 4, 'Implement automatic daily calibration cycle', @uid),
(@tid, @pid2, 'Capping', 'Cap cross-threading', 'Bottle rejected, product wasted', 'Cap chute misalignment', 'Visual check every 30 min', 5, 5, 3, 'Install cap alignment sensor with auto-reject', @uid);

-- Process Capability
INSERT INTO ProcessCapability (TenantId, ProjectId, MetricName, USL, LSL, Target, Mean, StdDev, Cp, Cpk, Ppk, SigmaLevel, DPMO, SampleSize, DataPoints, CreatedByUserId)
VALUES (@tid, @pid2, 'Fill Volume (ml)', 255.0, 245.0, 250.0, 258.2, 2.8, 1.19, 0.85, 0.80, 2.1, 48000.0, 80,
        '258,262,255,249,267,254,260,251,263,256,259,248,265,253,261,250,264,257,252,266,254,260,247,258,262,255,249,263,256,259,251,265,253,261,250,264,257,252,266,254,260,248,258,262,255,249,267,253,261,250,264,257,252,266,254,259,248,265,253,260,251,263,256,258,262,255,249,267,254,260,251,263,256,259,248,265,253,261,250,264',
        @uid);

-- VOC for Project 2
INSERT INTO VOC_Records (TenantId, ProjectId, CustomerName, VoiceStatement, CreatedByUserId)
VALUES
(@tid, @pid2, 'FDA Inspector', 'Fill weight records show underfill risk at 1.2%. Need corrective action plan.', @uid),
(@tid, @pid2, 'CFO Office', 'Product giveaway exceeded $180K this year on hot sauce line alone.', @uid);

-- COPQ
INSERT INTO COPQ (TenantId, ProjectId, EntryDate, AppraisalCosts, PreventionCosts, InternalFailureCosts, ExternalFailureCosts, Revenue, Notes, CreatedByUserId)
VALUES
(@tid, @pid2, DATE_SUB(@dt, INTERVAL 50 DAY), 3200.00, 1500.00, 15000.00, 2200.00, 890000.00, 'Baseline - overfill costs dominate', @uid),
(@tid, @pid2, DATE_SUB(@dt, INTERVAL 25 DAY), 2800.00, 2500.00, 12000.00, 1800.00, 895000.00, 'After initial nozzle maintenance', @uid);

-- Gantt
INSERT INTO GanttTasks (TenantId, ProjectId, TaskName, Phase, StartDate, EndDate, Progress, CreatedByUserId)
VALUES
(@tid, @pid2, 'Fill Weight Data Collection', 'Measure', DATE_SUB(@dt, INTERVAL 45 DAY), DATE_SUB(@dt, INTERVAL 35 DAY), 100, @uid),
(@tid, @pid2, 'Gage R&R on Checkweigher', 'Measure', DATE_SUB(@dt, INTERVAL 35 DAY), DATE_SUB(@dt, INTERVAL 30 DAY), 100, @uid),
(@tid, @pid2, 'Nozzle Seal Replacement', 'Analyze', DATE_SUB(@dt, INTERVAL 20 DAY), DATE_SUB(@dt, INTERVAL 15 DAY), 100, @uid),
(@tid, @pid2, 'Pressure Regulation Installation', 'Improve', DATE_ADD(@dt, INTERVAL 10 DAY), DATE_ADD(@dt, INTERVAL 18 DAY), 0, @uid),
(@tid, @pid2, 'Temperature Control Installation', 'Improve', DATE_ADD(@dt, INTERVAL 15 DAY), DATE_ADD(@dt, INTERVAL 25 DAY), 0, @uid),
(@tid, @pid2, 'SPC Implementation', 'Control', DATE_ADD(@dt, INTERVAL 30 DAY), DATE_ADD(@dt, INTERVAL 38 DAY), 0, @uid);

-- ============================================================
-- PROJECT 3: Packaging Seal Integrity (Black Belt)
-- Problem: 3.2% seal failure rate in snack packaging
-- ============================================================
INSERT INTO QuantarisProjects (TenantId, ProjectCode, ProjectName, Description, Status, Phase, Priority, FinancialImpact, StartDate, TargetEndDate, ProjectOwnerId, Champion, CreatedByUserId)
VALUES (@tid, 'FPC-2026-003', 'Snack Packaging Seal Integrity Improvement',
        'Reduce seal failure rate in snack packaging from 3.2% to <0.5%. Seal failures cause product spoilage, customer complaints, and brand damage. Current sigma level ~3.4.',
        'InProgress', 'Define', 'Critical', 415000.00, DATE_SUB(@dt, INTERVAL 20 DAY), DATE_ADD(@dt, INTERVAL 100 DAY), @uid, 'Jennifer Walsh - VP Quality', @uid);
SET @pid3 := LAST_INSERT_ID();

INSERT INTO ProjectCharters (TenantId, ProjectId, ProblemStatement, GoalStatement, BusinessCase, ScopeIncluded, ScopeExcluded, Timeline, ElevatorSpeech, CreatedByUserId)
VALUES (@tid, @pid3,
        'Seal failure rate across snack packaging lines is 3.2%, resulting in 12,800 defective packages per month. Customer complaints up 200% YoY. Retailer chargebacks total $95,000 annually.',
        'Reduce seal failure rate from 3.2% to <0.5% (Cp >= 1.67 for seal strength) within 100 days, eliminating retailer chargebacks.',
        'Annual savings of $415,000 from reduced waste ($210K), chargebacks ($95K), and complaint handling ($110K).',
        'All 4 packaging lines for chips, pretzels, and popcorn at the main plant. Sealing through inspection.',
        'Product cooking/kettle operations, raw material receiving, warehousing, distribution.',
        'Define: Week 1-3 | Measure: Week 4-7 | Analyze: Week 8-12 | Improve: Week 13-16 | Control: Week 17-20',
        'Three percent of our snack bags leak. That costs us $415K/year and angers customers. We will fix the sealing process with Six Sigma.',
        @uid);

-- FMEA for Project 3
INSERT INTO FMEA_Records (TenantId, ProjectId, ProcessStep, FailureMode, EffectOfFailure, CauseOfFailure, CurrentControl, Severity, Occurrence, Detection, RecommendedAction, CreatedByUserId)
VALUES
(@tid, @pid3, 'Heat Sealing', 'Insufficient seal temperature', 'Weak seal opens during transport', 'Heater band degradation, temperature controller drift', 'Weekly temperature check with thermocouple', 9, 7, 5, 'Install continuous temperature monitoring with auto-adjustment', @uid),
(@tid, @pid3, 'Heat Sealing', 'Excessive seal temperature', 'Film melts through, creates pinhole leaks', 'Temperature controller overshoot', 'Visual inspection every 30 min', 8, 5, 6, 'Implement PID temperature control upgrade', @uid),
(@tid, @pid3, 'Heat Sealing', 'Inconsistent seal pressure', 'Variable seal strength across width', 'Pneumatic pressure fluctuation, seal bar wear', 'Pressure gauge reading (manual)', 8, 6, 5, 'Install electronic pressure regulators with data logging', @uid),
(@tid, @pid3, 'Film Feeding', 'Film wrinkle at seal area', 'Wrinkles create leak paths in seal', 'Misaligned film rollers, tension variation', 'Operator adjusts tension manually', 7, 6, 6, 'Install auto-tension control with edge guide sensors', @uid),
(@tid, @pid3, 'Seal Bar', 'Seal bar surface contamination', 'Product residue prevents proper seal', 'Oil/powder buildup on seal bar', 'Cleaned every shift change', 7, 7, 4, 'Implement automated cleaning cycle between batches', @uid),
(@tid, @pid3, 'Inspection', 'Leak tester false passes', 'Leaking packages reach customer', 'Leak tester calibration drift, hole size threshold too large', 'Monthly calibration check', 9, 4, 8, 'Upgrade to vacuum decay leak test with daily verification', @uid);

-- Process Capability
INSERT INTO ProcessCapability (TenantId, ProjectId, MetricName, USL, LSL, Target, Mean, StdDev, Cp, Cpk, Ppk, SigmaLevel, DPMO, SampleSize, DataPoints, CreatedByUserId)
VALUES (@tid, @pid3, 'Seal Strength (N/cm)', 25.0, 15.0, 20.0, 18.5, 3.2, 0.52, 0.36, 0.31, 3.4, 32000.0, 60,
        '22.1,19.5,16.3,24.8,14.2,20.6,18.9,15.4,23.1,17.8,21.4,19.2,16.7,25.3,13.8,22.5,18.1,20.3,15.9,24.2,17.1,21.8,19.6,14.5,23.7,16.4,20.9,18.4,15.2,25.1,13.5,22.8,19.1,21.2,16.8,24.5,17.5,20.1,18.7,14.8,23.4,16.1,21.5,19.8,15.6,24.1,17.3,20.7,18.2,14.1,23.9,16.9,21.1,19.4,15.1,24.6,17.6,20.4,18.5,14.6',
        @uid);

-- VOC for Project 3
INSERT INTO VOC_Records (TenantId, ProjectId, CustomerName, VoiceStatement, CreatedByUserId)
VALUES
(@tid, @pid3, 'Walmart DC', 'Multiple cases of crushed, leaking snack bags received. Product spoiled. Issuing chargeback.', @uid),
(@tid, @pid3, 'Kroger Regional', 'Customers complaining about stale chips from opened bags. Seal integrity issue suspected.', @uid),
(@tid, @pid3, 'Target Quality Team', 'Our audit found 4.1% seal failure rate in your latest shipment. Needs immediate attention.', @uid);

-- COPQ
INSERT INTO COPQ (TenantId, ProjectId, EntryDate, AppraisalCosts, PreventionCosts, InternalFailureCosts, ExternalFailureCosts, Revenue, Notes, CreatedByUserId)
VALUES
(@tid, @pid3, DATE_SUB(@dt, INTERVAL 20 DAY), 8500.00, 4000.00, 28500.00, 32000.00, 2500000.00, 'Baseline - high external failure costs from retailer chargebacks', @uid);

-- Gantt
INSERT INTO GanttTasks (TenantId, ProjectId, TaskName, Phase, StartDate, EndDate, Progress, CreatedByUserId)
VALUES
(@tid, @pid3, 'Project Charter & Team Formation', 'Define', DATE_SUB(@dt, INTERVAL 20 DAY), DATE_SUB(@dt, INTERVAL 15 DAY), 100, @uid),
(@tid, @pid3, 'Current State Mapping', 'Define', DATE_SUB(@dt, INTERVAL 15 DAY), DATE_SUB(@dt, INTERVAL 10 DAY), 100, @uid),
(@tid, @pid3, 'Seal Strength Measurement System', 'Measure', DATE_SUB(@dt, INTERVAL 8 DAY), DATE_SUB(@dt, INTERVAL 3 DAY), 80, @uid),
(@tid, @pid3, 'Data Collection Plan', 'Measure', DATE_SUB(@dt, INTERVAL 3 DAY), DATE_ADD(@dt, INTERVAL 5 DAY), 20, @uid),
(@tid, @pid3, 'Seal Failure Stratification', 'Analyze', DATE_ADD(@dt, INTERVAL 5 DAY), DATE_ADD(@dt, INTERVAL 15 DAY), 0, @uid),
(@tid, @pid3, 'DOE on Seal Parameters', 'Analyze', DATE_ADD(@dt, INTERVAL 15 DAY), DATE_ADD(@dt, INTERVAL 28 DAY), 0, @uid),
(@tid, @pid3, 'Temperature Control Upgrade', 'Improve', DATE_ADD(@dt, INTERVAL 30 DAY), DATE_ADD(@dt, INTERVAL 45 DAY), 0, @uid),
(@tid, @pid3, 'Pilot & Validate', 'Improve', DATE_ADD(@dt, INTERVAL 45 DAY), DATE_ADD(@dt, INTERVAL 60 DAY), 0, @uid),
(@tid, @pid3, 'Control Plan & SPC', 'Control', DATE_ADD(@dt, INTERVAL 65 DAY), DATE_ADD(@dt, INTERVAL 80 DAY), 0, @uid);

INSERT INTO GanttMilestones (TenantId, ProjectId, MilestoneName, MilestoneDate, CreatedByUserId)
VALUES
(@tid, @pid3, 'Charter Approved', DATE_SUB(@dt, INTERVAL 15 DAY), @uid),
(@tid, @pid3, 'Measurement System OK', DATE_SUB(@dt, INTERVAL 3 DAY), @uid),
(@tid, @pid3, 'Root Causes Identified', DATE_ADD(@dt, INTERVAL 28 DAY), @uid),
(@tid, @pid3, 'Improvement Implemented', DATE_ADD(@dt, INTERVAL 60 DAY), @uid),
(@tid, @pid3, 'Project Closure', DATE_ADD(@dt, INTERVAL 100 DAY), @uid);

-- Root Cause for Project 3
INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, Effect, IsFishbone, CreatedByUserId)
VALUES
(@tid, @pid3, 'Machines', 'Seal bar temperature drifts over production run', 'Seal strength variation', true, @uid),
(@tid, @pid3, 'Machines', 'Pneumatic pressure not consistent across all seal heads', 'Seal strength variation', true, @uid),
(@tid, @pid3, 'Methods', 'No standard procedure for seal bar cleaning frequency', 'Seal contamination', true, @uid),
(@tid, @pid3, 'Methods', 'Changeover procedure does not include seal verification', 'Setup errors', true, @uid),
(@tid, @pid3, 'Materials', 'Film lot-to-lot variation in seal layer thickness', 'Inconsistent seal quality', true, @uid),
(@tid, @pid3, 'Materials', 'Product dust/oil on film surface varies by product type', 'Seal contamination', true, @uid),
(@tid, @pid3, 'Measurement', 'Leak tester sensitivity not validated daily', 'False passes', true, @uid),
(@tid, @pid3, 'Measurement', 'Seal strength test method not standardized across shifts', 'Measurement error', true, @uid),
(@tid, @pid3, 'People', 'Operators trained differently on seal bar adjustment', 'Inconsistent operation', true, @uid),
(@tid, @pid3, 'People', 'No visual standard for acceptable seal appearance', 'Subjective judgement', true, @uid);

-- ============================================================
-- Sample Size Calculation (Project 2)
-- ============================================================
INSERT INTO SampleSizeCalculations (TenantId, ProjectId, CalcName, CalcType, Parameters, Result, CreatedByUserId)
VALUES (@tid, @pid2, 'Fill Weight Sampling Plan', 'Continuous', '{"ConfidenceLevel":95,"MarginOfError":0.5,"StdDev":2.8,"Population":50000}', 121, @uid);

-- ============================================================
-- Hoshin Kanri (Strategic alignment for all 3 projects)
-- ============================================================
INSERT INTO HoshinKanriModels (TenantId, ProjectId, ModelName, FiscalYear, CreatedByUserId)
VALUES (@tid, @pid1, '2026 Food Company Quality Transformation', 2026, @uid);

-- ============================================================
-- Benefits Tracking
-- ============================================================
INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, CreatedByUserId)
VALUES (@tid, @pid1, 'Reduced Waste from Thickness Rejects', 'Savings', 'Direct', 95000.00, 'USD', @dt, DATE_ADD(@dt, INTERVAL 365 DAY), 'Planned', @uid);
INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, CreatedByUserId)
VALUES (@tid, @pid1, 'Reduced Customer Complaint Handling', 'Savings', 'Indirect', 52000.00, 'USD', @dt, DATE_ADD(@dt, INTERVAL 365 DAY), 'Planned', @uid);
INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, CreatedByUserId)
VALUES (@tid, @pid1, 'Increased Production Throughput', 'Revenue', 'Direct', 38000.00, 'USD', DATE_ADD(@dt, INTERVAL 90 DAY), DATE_ADD(@dt, INTERVAL 365 DAY), 'Planned', @uid);
INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, CreatedByUserId)
VALUES (@tid, @pid3, 'Reduced Seal Waste', 'Savings', 'Direct', 210000.00, 'USD', DATE_ADD(@dt, INTERVAL 60 DAY), DATE_ADD(@dt, INTERVAL 425 DAY), 'Planned', @uid);
INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, CreatedByUserId)
VALUES (@tid, @pid3, 'Eliminated Retailer Chargebacks', 'CostAvoidance', 'Direct', 95000.00, 'USD', DATE_ADD(@dt, INTERVAL 60 DAY), DATE_ADD(@dt, INTERVAL 425 DAY), 'Planned', @uid);
INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, CreatedByUserId)
VALUES (@tid, @pid3, 'Reduced Complaint Handling Cost', 'Savings', 'Indirect', 110000.00, 'USD', DATE_ADD(@dt, INTERVAL 60 DAY), DATE_ADD(@dt, INTERVAL 425 DAY), 'Planned', @uid);

-- Audit log
INSERT INTO AuditLogs (TenantId, UserId, Action, EntityType, EntityId, Description, CreatedOn)
VALUES (@tid, @uid, 'Seed', 'FoodCompanyDemo', NULL, 'Seeded 3 food company Six Sigma demo projects with full DMAIC data', CURRENT_TIMESTAMP);
