-- Quantaris - Control Charts (MySQL)
CREATE TABLE IF NOT EXISTS ControlChartModels (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ChartName VARCHAR(200) NOT NULL,
    ChartType VARCHAR(20) NOT NULL,
    SubgroupSize INT DEFAULT 1,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_cchart_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_cchart_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_cchart_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ControlChartData (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    SampleIndex INT NOT NULL,
    SampleValues TEXT NOT NULL,
    CONSTRAINT fk_cchartdata_model FOREIGN KEY (ModelId) REFERENCES ControlChartModels(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ControlChartLimits (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    CenterLine DOUBLE NOT NULL,
    UCL DOUBLE NOT NULL,
    LCL DOUBLE NOT NULL,
    CenterLineR DOUBLE,
    UCLR DOUBLE,
    LCLR DOUBLE,
    CONSTRAINT fk_cchartlimits_model FOREIGN KEY (ModelId) REFERENCES ControlChartModels(Id) ON DELETE CASCADE
) ENGINE=InnoDB;
