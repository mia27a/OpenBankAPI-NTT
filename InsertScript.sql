insert into Users(Email, Full_Name, Username, Password_hash, Created_at, Password_changed_at)
values
	('marianavgamorim@gmail.com', 'Mariana Viegas Guedes de Amorim', 'mariana27a', '$2a$11$6VUZEUhzp7kO.nRoIhflbuE4QK5d8eriJrAu9R9n/aDPwpI.4k4ia', default, default),
	('adrianagamorim@gmail.com', 'Adriana Guedes de Amorim', 'adri-Aga', '$2a$11$TYYId16KMVl3wiGlbJ0td.Y6qHaaHiCCO8aVlNmcp61MhAml/IC46', default, default),
	('mrlawrenceb@hotmail.com', 'Lawrence Michael Ballayard', 'mrLaw123', '$2a$11$XRobh7ZGCdPTIaVa9yKhL.JUF0xe44PS8XAEHufi/DWdNCH/Ql78.', default, default);
	
insert into Accounts (User_Id, Balance, Currency, Created_at)
values
	(1, 1050, default, default),
	(2, 2950, default, default),
	(3, 20000, 'DOL', default);
	
insert into Transactions (Account_id, Amount, Balance, Created_at)
values
	(100000000, '1000', 1000, default),
	(100000001, '3000', 3000, default),
	(100000002, '20000', 20000, default),
	(100000000, '50', 1050, default),
	(100000001, '-50', 2950, default);
	
insert into Transfers (Amount, Currency, From_account_id, To_account_id, Created_at)
values 
	(+50, default, 100000001, 100000000, default);