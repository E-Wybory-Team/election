DROP DATABASE `freedb_Wybory`;
CREATE DATABASE `freedb_Wybory`;
USE `freedb_Wybory`;	

DROP TABLE IF EXISTS `People`;
CREATE TABLE `People`(
`idPerson` INT NOT NULL AUTO_INCREMENT,
`name` VARCHAR(30) NOT NULL,
`surname` VARCHAR(50) NOT NULL,
`pesel` CHAR(11) NOT NULL,
`birthDate` DATE NOT NULL,
CONSTRAINT `PkPeople` primary key(`idPerson`)
);

DROP TABLE IF EXISTS `Parties`;
CREATE TABLE `Parties`(
`partyName` VARCHAR(50) NOT NULL,
`abbreviation` VARCHAR(10),
`partyAddress` VARCHAR(100),
`partyType` VARCHAR(30) NOT NULL,
`isCoalition` BOOLEAN NOT NULL,
CONSTRAINT `PkParties` primary key(`partyName`)
);

DROP TABLE IF EXISTS `UserTypes`;
CREATE TABLE `UserTypes`(
`userTypeName` VARCHAR(30) NOT NULL,
`userTypeInfo` VARCHAR(255),
CONSTRAINT `PkUserTypes` primary key(`userTypeName`)
);

DROP TABLE IF EXISTS `ElectionTypes`;
CREATE TABLE `ElectionTypes`(
`electionTypeName` VARCHAR(50) NOT NULL,
`electionTypeInfo` VARCHAR(255),
CONSTRAINT `PkElectionTypes` primary key(`electionTypeName`)
);

DROP TABLE IF EXISTS `Constituences`;
CREATE TABLE `Constituences`(
`idConstituency` INT NOT NULL AUTO_INCREMENT,
`constituencyName` VARCHAR(50) NOT NULL,
CONSTRAINT `PkConstituences` primary key(`idConstituency`)
);

DROP TABLE IF EXISTS `Districts`;
CREATE TABLE `Districts`(
`idDistrict` INT NOT NULL AUTO_INCREMENT,
`districtName` VARCHAR(50) NOT NULL,
`idConstituency` INT NOT NULL,
CONSTRAINT `FkConstituencyDistrict` FOREIGN KEY (`idConstituency`) 
	REFERENCES `Constituences`(`idConstituency`)
    ON DELETE RESTRICT,
CONSTRAINT `PkDistricts` primary key(`idDistrict`)
);

DROP TABLE IF EXISTS `ElectionUsers`;
CREATE TABLE `ElectionUsers`(
`email` VARCHAR(50) NOT NULL,
`phoneNumber` VARCHAR(15) NOT NULL,
`password` VARCHAR(100) NOT NULL,
`idPerson` INT NOT NULL,
`idDistrict` INT NOT NULL,
CONSTRAINT 	`FkPeopleElectionUsers` FOREIGN KEY (`idPerson`)
	REFERENCES `People`(`idPerson`)
	ON DELETE RESTRICT,
CONSTRAINT `FkDistrictELectionUsers` FOREIGN KEY (`idDistrict`) 
	REFERENCES `Districts`(`idDistrict`)
    ON DELETE RESTRICT,
CONSTRAINT `PkElectionUsers` primary key(`email`)
);

DROP TABLE IF EXISTS `UserTypeSets`;
CREATE TABLE `UserTypeSets`(
`email` VARCHAR(50) NOT NULL,
`userTypeName` VARCHAR(30) NOT NULL,
CONSTRAINT `FkPElectionUsersUserTypeSets` FOREIGN KEY (`email`) 
	REFERENCES `ElectionUsers`(`email`)
    ON DELETE CASCADE,
CONSTRAINT `UserTypesUserTypeSets` FOREIGN KEY(`userTypeName`)
	REFERENCES `UserTypes`(`userTypeName`)
    ON DELETE CASCADE,
CONSTRAINT `UserTypeSetsPK` PRIMARY KEY(`email`,`userTypeName`)
);

DROP TABLE IF EXISTS `Elections`;
CREATE TABLE `Elections`(
`idElection` INT NOT NULL AUTO_INCREMENT,
`electionStartDate` DATE NOT NULL,
`electionEndDate` DATE NOT NULL,
`electionTour` TINYINT,
`electionTypeName` VARCHAR(50) NOT NULL,
CONSTRAINT `fKElectionTypeNameElections` FOREIGN KEY(`electionTypeName`)
	REFERENCES `ElectionTypes`(`electionTypeName`)
	ON DELETE RESTRICT,
CONSTRAINT `EndDateNotLaterThanStartDate` CHECK(`electionStartDate` < `electionEndDate`) ,
CONSTRAINT `PkElections` primary key(`idElection`)
);

DROP TABLE IF EXISTS `Candidates`;
CREATE TABLE `Candidates`(
`idCandidate` INT NOT NULL AUTO_INCREMENT,
`campaignDescription` VARCHAR(255),
`jobType` VARCHAR(50) NOT NULL,
`placeOfResidence` VARCHAR(50) NOT NULL,
`idPerson` INT NOT NULL,
`idDistrict` INT NOT NULL,
`partyName` VARCHAR(50),
CONSTRAINT `FkPeopleCandidates` FOREIGN KEY (`idPerson`)
	REFERENCES `People`(`idPerson`)
    ON DELETE RESTRICT,
CONSTRAINT `FkDistrictsCandidates` FOREIGN KEY (`idDistrict`)
	REFERENCES `Districts`(`idDistrict`)
    ON DELETE RESTRICT,
CONSTRAINT `FkPartiesCandidates` FOREIGN KEY (`partyName`)
	REFERENCES `Parties`(`partyName`)
    ON DELETE RESTRICT,
CONSTRAINT `PkCandidates` primary key(`idCandidate`)
);

DROP TABLE IF EXISTS `Candidatures`;
CREATE TABLE `Candidatures`(
`idCandidate` INT NOT NULL,
`idElection` INT NOT NULL,
`votesGained` INT NOT NULL,
CONSTRAINT `FkCandidatesCandidatures` FOREIGN KEY (`idCandidate`) 
	REFERENCES `Candidates`(`idCandidate`)
    ON DELETE CASCADE,
CONSTRAINT `FkElectionsCandidatures` FOREIGN KEY(`idElection`)
	REFERENCES `Elections`(`idElection`)
    ON DELETE CASCADE,
CONSTRAINT `CandidaturesPK` PRIMARY KEY(`idCandidate`,`idElection`)
);

DROP TABLE IF EXISTS `Votes`;
CREATE TABLE `Votes`(
`idCandidate` INT NOT NULL,
`idElection` INT NOT NULL,
`email` VARCHAR(50) NOT NULL,
`isValid` BOOLEAN NOT NULL,
CONSTRAINT `FkCandidatesVotes` FOREIGN KEY (`idCandidate`) 
	REFERENCES `Candidates`(`idCandidate`)
    ON DELETE RESTRICT,
CONSTRAINT `FkElectionsVotes` FOREIGN KEY(`idElection`)
	REFERENCES `Elections`(`idElection`)
    ON DELETE RESTRICT,
CONSTRAINT `FkElectionUsersVotes` FOREIGN KEY(`email`)
	REFERENCES `ElectionUsers`(`email`)
    ON DELETE RESTRICT,
CONSTRAINT `VotesPK` PRIMARY KEY(`email`,`idElection`)
);