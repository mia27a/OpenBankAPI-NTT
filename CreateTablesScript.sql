drop table if exists Refresh_token;
drop table if exists Transfers;
drop table if exists Transactions;
drop table if exists Accounts;
drop table if exists Users;

create table Users(
	Id					SERIAL 			primary key,
	Email				VARCHAR(254)	not null	UNIQUE,
	Full_Name			VARCHAR(100)	not null	check(length(Full_Name) >= 16),
	Username			VARCHAR(50)		not null 	check(length(Username) >= 8)	UNIQUE,
	Password_hash		VARCHAR(200),
	Created_at			TIMESTAMP		not null	default (CURRENT_TIMESTAMP),
	Password_changed_at	TIMESTAMP		not null	default (CURRENT_TIMESTAMP)
);

create table Accounts(
	Id					SERIAL 		primary key,
	User_Id				INT 		not null	references Users(Id),
	Balance				numeric 	not null	check(Balance >= 0),
	Currency			CHAR(3) 	not null 	default ('EUR'),
	Created_at			TIMESTAMP	not null	default (CURRENT_TIMESTAMP)
);

ALTER SEQUENCE Accounts_id_seq RESTART WITH 100000000 INCREMENT BY 1;

create table Transactions(
	Id					SERIAL 			primary key,
	Account_id			INT 			not null	references Accounts(Id),
	Amount				numeric			not null,
	Balance				numeric 		not null	check(Balance >= 0),
	Created_at			TIMESTAMP		not null	default (CURRENT_TIMESTAMP)
);

create table Transfers(
	Id					SERIAL 			primary key,
	Amount				numeric 		not null,
	Currency			CHAR(3) 		not null 	default ('EUR'),	
	From_account_id		INT 			not null,
	To_account_id		INT 			not null,
	Created_at			TIMESTAMP		not null	default (CURRENT_TIMESTAMP)
);

create table Refresh_token(
	Id					SERIAL 			primary key,
	User_Id				INT 			not null		references Users(Id),
	Token				VARCHAR(254)	not null,
	Expires				TIMESTAMP		not null,
	Created				TIMESTAMP		not null,
	Created_by_ip		VARCHAR(200)	not null,
	Revoked				TIMESTAMP,
	Revoked_by_ip		VARCHAR(200),
	Replaced_by_token	VARCHAR(200),
	Reason_revoked		VARCHAR(200),
	Is_expired			BOOLEAN,
	Is_revoked			BOOLEAN,
	Is_active			BOOLEAN
);