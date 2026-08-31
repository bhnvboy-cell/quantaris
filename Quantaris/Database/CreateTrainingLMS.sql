-- Quantaris - Training & Certification LMS (MySQL)
CREATE TABLE IF NOT EXISTS TrainingCourses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    CourseCode VARCHAR(20) NOT NULL,
    CourseName VARCHAR(200) NOT NULL,
    BeltLevel VARCHAR(20),
    Description TEXT,
    DurationHours DOUBLE,
    IsActive TINYINT(1) DEFAULT 1,
    CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_tcourse_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS TrainingCertifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    TenantId INT NOT NULL,
    UserId INT NOT NULL,
    CertificationName VARCHAR(200) NOT NULL,
    BeltLevel VARCHAR(20) NOT NULL,
    IssueDate DATE NOT NULL,
    ExpiryDate DATE,
    IssuedBy VARCHAR(200),
    IsActive TINYINT(1) DEFAULT 1,
    CONSTRAINT fk_tcert_tenant FOREIGN KEY (TenantId) REFERENCES Tenants(Id),
    CONSTRAINT fk_tcert_user FOREIGN KEY (UserId) REFERENCES Users(Id)
) ENGINE=InnoDB;
