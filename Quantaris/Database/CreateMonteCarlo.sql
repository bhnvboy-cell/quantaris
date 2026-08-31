-- Quantaris - Monte Carlo Simulation (MySQL)
CREATE TABLE IF NOT EXISTS MonteCarloModels (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ModelName VARCHAR(200) NOT NULL,
    Iterations INT NOT NULL DEFAULT 10000,
    OutputFormula TEXT,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_mc_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_mc_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_mc_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS MonteCarloInputs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    InputName VARCHAR(100) NOT NULL,
    DistributionType VARCHAR(20) NOT NULL,
    Param1 DOUBLE NOT NULL,
    Param2 DOUBLE,
    Param3 DOUBLE,
    CONSTRAINT fk_mci_model FOREIGN KEY (ModelId) REFERENCES MonteCarloModels(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS MonteCarloResults (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    ResultIndex INT NOT NULL,
    ResultValue DOUBLE NOT NULL,
    CONSTRAINT fk_mcr_model FOREIGN KEY (ModelId) REFERENCES MonteCarloModels(Id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE INDEX idx_mc_model ON MonteCarloResults(ModelId);
