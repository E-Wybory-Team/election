DROP TABLE IF EXISTS `People`;
CREATE TABLE `People`(
`idPerson` INT NOT NULL AUTO_INCREMENT,
`name` VARCHAR(30) NOT NULL,
`surname` VARCHAR(50) NOT NULL,
`pesel` CHAR(11) NOT NULL,
`birthDate` DATE NOT NULL,
CONSTRAINT `PkPeople` PRIMARY KEY(`idPerson`),
CONSTRAINT `uniquePesel` UNIQUE (`pesel`)
);

DROP TABLE IF EXISTS `Parties`;
CREATE TABLE `Parties`(
`idParty` INT AUTO_INCREMENT NOT NULL,
`partyName` VARCHAR(50) NOT NULL,
`abbreviation` VARCHAR(10),
`partyAddress` VARCHAR(100),
`partyType` VARCHAR(30) NOT NULL,
`isCoalition` BOOLEAN NOT NULL,
`listCommiteeNumber` INT,
`website` VARCHAR(2000),
CONSTRAINT `PkParties` primary key(`idParty`),
CONSTRAINT `uniquePartyName` UNIQUE (`partyName`)
);

DROP TABLE IF EXISTS `UserTypesGroups`;
CREATE TABLE `UserTypesGroups`(
`idUserTypesGroup` INT AUTO_INCREMENT NOT NULL,
`userTypesGroupName` VARCHAR(30) NOT NULL,
`userTypesGroupInfo` VARCHAR(255),
CONSTRAINT `PkUserTypesGroup` primary key(`idUserTypesGroup`),
CONSTRAINT `uniqueUserTypeName` UNIQUE (`userTypesGroupName`)
);

CREATE TABLE `UserTypes`(
`idUserType` INT AUTO_INCREMENT NOT NULL,
`userTypeName` VARCHAR(30) NOT NULL,
`userTypeInfo` VARCHAR(255),
`idUserTypesGroup` INT NOT NULL,
CONSTRAINT `PkUserTypes` primary key(`idUserType`),
CONSTRAINT `uniqueUserTypeName` UNIQUE (`userTypeName`),
CONSTRAINT `FkUserTypesGroupsUserTypes` FOREIGN KEY (`idUserTypesGroup`)
	REFERENCES `UserTypesGroups`(`idUserTypesGroup`)
    ON DELETE RESTRICT
);

DROP TABLE IF EXISTS `ElectionTypes`;
CREATE TABLE `ElectionTypes`(
`idElectionType` INT AUTO_INCREMENT NOT NULL,
`electionTypeName` VARCHAR(50) NOT NULL,
`electionTypeInfo` VARCHAR(255),
CONSTRAINT `PkElectionTypes` primary key(`idElectionType`),
CONSTRAINT `uniqueElectionTypeName` UNIQUE (`electionTypeName`)
);

DROP TABLE IF EXISTS `Constituences`;
CREATE TABLE `Constituences`(
`idConstituency` INT NOT NULL AUTO_INCREMENT,
`constituencyName` VARCHAR(50) NOT NULL,
CONSTRAINT `PkConstituences` primary key(`idConstituency`)
);

DROP TABLE IF EXISTS `Voivodeships`;
CREATE TABLE `Voivodeships`(
`idVoivodeship` INT NOT NULL AUTO_INCREMENT,
`voivodeshipName` VARCHAR(50) NOT NULL,
CONSTRAINT `PkVoivodeships` primary key(`idVoivodeship`)
);

DROP TABLE IF EXISTS `Counties`;
CREATE TABLE `Counties`(
`idCounty` INT NOT NULL AUTO_INCREMENT,
`countyName` VARCHAR(50) NOT NULL,
`idVoivodeship` INT NOT NULL,
CONSTRAINT `PkCounties` primary key(`idCounty`),
CONSTRAINT `FkVoivodeshipsCounties` FOREIGN KEY (`idVoivodeship`)
	REFERENCES `Voivodeships`(`idVoivodeship`)
    ON DELETE RESTRICT,
CONSTRAINT `uniqueCountyNameInVoivodeship` UNIQUE (`countyName`, `idVoivodeship`)
);

DROP TABLE IF EXISTS `Provinces`;
CREATE TABLE `Provinces`(
`idProvince` INT NOT NULL AUTO_INCREMENT,
`provinceName` VARCHAR(50) NOT NULL,
`idCounty` INT NOT NULL,
CONSTRAINT `PkProvinces` primary key(`idProvince`),
CONSTRAINT `FkCountyProvinces` FOREIGN KEY(`idCounty`)
	REFERENCES `Counties`(`idCounty`)
    ON DELETE RESTRICT,
CONSTRAINT `uniqueProvinceNameInCounty` UNIQUE (`provinceName`, `idCounty`)
);

DROP TABLE IF EXISTS `Districts`;
CREATE TABLE `Districts`(
`idDistrict` INT NOT NULL AUTO_INCREMENT,
`districtName` VARCHAR(50) NOT NULL,
`disabledFacilities` BOOLEAN NOT NULL,
`districtHeadquarters` VARCHAR(150) NOT NULL,
`idConstituency` INT NOT NULL,
`idProvince` INT,
CONSTRAINT `FkConstituencyDistrict` FOREIGN KEY (`idConstituency`) 
	REFERENCES `Constituences`(`idConstituency`)
    ON DELETE RESTRICT,
CONSTRAINT `FkProvincesDistrict` FOREIGN KEY (`idProvince`) 
	REFERENCES `Provinces`(`idProvince`)
    ON DELETE SET NULL,
CONSTRAINT `uniqueDistrictNameInConstituency` UNIQUE (`districtName`, `idConstituency`),
CONSTRAINT `PkDistricts` primary key(`idDistrict`)
);

DROP TABLE IF EXISTS `ElectionUsers`;
CREATE TABLE `ElectionUsers`(
`idElectionUser` INT AUTO_INCREMENT NOT NULL,
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
CONSTRAINT `PkElectionUsers` primary key(`idElectionUser`),
CONSTRAINT `uniqueEmail` UNIQUE (`email`),
CONSTRAINT `uniquePerson` UNIQUE (`idPerson`)
);

DROP TABLE IF EXISTS `UserTypeSets`;
CREATE TABLE `UserTypeSets`(
`idUserTypeSet` INT AUTO_INCREMENT NOT NULL,
`idElectionUser` INT NOT NULL,
`idUserType` INT NOT NULL,
CONSTRAINT `FkElectionUsersUserTypeSets` FOREIGN KEY (`idElectionUser`) 
	REFERENCES `ElectionUsers`(`idElectionUser`)
    ON DELETE CASCADE,
CONSTRAINT `UserTypesUserTypeSets` FOREIGN KEY(`idUserType`)
	REFERENCES `UserTypes`(`idUserType`)
    ON DELETE CASCADE,
CONSTRAINT `PkUserTypeSets` PRIMARY KEY(`idUserTypeSet`),
CONSTRAINT `uniqueElectionUserUserType` UNIQUE(`idElectionUser`,`idUserType`)
);

DROP TABLE IF EXISTS `Elections`;
CREATE TABLE `Elections`(
`idElection` INT NOT NULL AUTO_INCREMENT,
`electionStartDate` DATETIME NOT NULL,
`electionEndDate` DATETIME NOT NULL,
`electionTour` INT,
`idElectionType` INT NOT NULL,
CONSTRAINT `fkElectionTypeElections` FOREIGN KEY(`idElectionType`)
	REFERENCES `ElectionTypes`(`idElectionType`)
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
`positionNumber` INT NOT NULL,
`educationStatus` VARCHAR(50),
`workplace` VARCHAR(100),
`idPerson` INT NOT NULL,
`idDistrict` INT NOT NULL,
`idParty` INT,
`idElection` INT NOT NULL,
CONSTRAINT `FkPeopleCandidates` FOREIGN KEY (`idPerson`)
	REFERENCES `People`(`idPerson`)
    ON DELETE RESTRICT,
CONSTRAINT `FkDistrictsCandidates` FOREIGN KEY (`idDistrict`)
	REFERENCES `Districts`(`idDistrict`)
    ON DELETE RESTRICT,
CONSTRAINT `FkPartiesCandidates` FOREIGN KEY (`idParty`)
	REFERENCES `Parties`(`idParty`)
    ON DELETE SET NULL,
CONSTRAINT `FkElectionsCandidates` FOREIGN KEY (`idElection`)
	REFERENCES `Elections`(`idElection`)
    ON DELETE RESTRICT,
CONSTRAINT `PkCandidates` primary key(`idCandidate`)
);

DROP TABLE IF EXISTS `Voters`;
CREATE TABLE `Voters`(
`idVoter` INT AUTO_INCREMENT NOT NULL,
`idElectionUser` INT NOT NULL,
`idDistrict` INT NOT NULL,
CONSTRAINT `FkElectionUsersVoters` FOREIGN KEY(`idElectionUser`)
	REFERENCES `ElectionUsers`(`idElectionUser`)
    ON DELETE CASCADE,
CONSTRAINT `FkDistrictsVoters` FOREIGN KEY(`idDistrict`)
	REFERENCES `Districts`(`idDistrict`)
    ON DELETE RESTRICT,
CONSTRAINT `uniqueElectionUser` UNIQUE (`idElectionUser`),
CONSTRAINT `PkVoters` PRIMARY KEY (`idVoter`)
);

DROP TABLE IF EXISTS `Votes`;
CREATE TABLE `Votes`(
`idVote` INT NOT NULL AUTO_INCREMENT,
`isValid` BOOLEAN NOT NULL,
`idCandidate` INT NOT NULL,
`idElection` INT NOT NULL,
CONSTRAINT `FkCandidatesVotes` FOREIGN KEY (`idCandidate`) 
	REFERENCES `Candidates`(`idCandidate`)
    ON DELETE RESTRICT,
CONSTRAINT `FkElectionsVotes` FOREIGN KEY(`idElection`)
	REFERENCES `Elections`(`idElection`)
    ON DELETE RESTRICT,
CONSTRAINT `VotesPK` PRIMARY KEY(`idVote`)
);


DROP TABLE IF EXISTS `ElectionVoters`;
CREATE TABLE `ElectionVoters`(
`idElectionVoter` INT NOT NULL AUTO_INCREMENT,
`idElection` INT NOT NULL,
`idVoter` INT NOT NULL,
CONSTRAINT `FkElectionElectionVoters` FOREIGN KEY(`idElection`)
	REFERENCES `Elections`(`idElection`)
	ON DELETE CASCADE,
CONSTRAINT `FkVoterElectionVoters` FOREIGN KEY(`idVoter`)
	REFERENCES `Voters`(`idVoter`)
	ON DELETE CASCADE,
CONSTRAINT `PkElectionVoters` PRIMARY KEY (`idElectionVoter`)
);
