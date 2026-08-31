-- Quantaris - Regression Analysis (MySQL)
CREATE TABLE IF NOT EXISTS RegressionModels (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ModelName VARCHAR(200) NOT NULL,
    ResponseName VARCHAR(100) NOT NULL,
    PredictorNames TEXT NOT NULL,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_reg_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_reg_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_reg_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS RegressionData (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    DataIndex INT NOT NULL,
    ResponseValue DOUBLE NOT NULL,
    PredictorValues TEXT NOT NULL,
    CONSTRAINT fk_regdata_model FOREIGN KEY (ModelId) REFERENCES RegressionModels(Id) ON DELETE CASCADE
) ENGINE=InnoDB;