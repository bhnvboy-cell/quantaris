-- Quantaris - Hoshin Kanri (MySQL)
CREATE TABLE IF NOT EXISTS HoshinKanriModels (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    ProjectId INT NOT NULL,
    ModelName VARCHAR(200) NOT NULL,
    FiscalYear INT NOT NULL,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedByUserId INT,
    CONSTRAINT fk_hk_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_hk_project FOREIGN KEY (ProjectId) REFERENCES QuantarisProjects(Id),
    CONSTRAINT fk_hk_user FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS HoshinKanriItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ModelId INT NOT NULL,
    ItemType VARCHAR(20) NOT NULL,
    ItemIndex INT NOT NULL,
    ItemName TEXT NOT NULL,
    Owner VARCHAR(100),
    TargetDate DATE,
    Metrics TEXT,
    Progress DOUBLE DEFAULT 0,
    Status VARCHAR(20) DEFAULT 'NotStarted',
    ParentId INT,
    CONSTRAINT fk_hkitems_model FOREIGN KEY (ModelId) REFERENCES HoshinKanriModels(Id) ON DELETE CASCADE,
    CONSTRAINT fk_hkitems_parent FOREIGN KEY (ParentId) REFERENCES HoshinKanriItems(Id)
) ENGINE=InnoDB;
