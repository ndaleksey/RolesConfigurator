SET search_path TO 'permission';

INSERT INTO role_subsystem_permission (role_id, subsystem_number) VALUES 
	-- Командир
	('50622c4e-7d18-298c-7b8c-c2530e83aa76', 0),
	('50622c4e-7d18-298c-7b8c-c2530e83aa76', 1),
	('50622c4e-7d18-298c-7b8c-c2530e83aa76', 2),
	('50622c4e-7d18-298c-7b8c-c2530e83aa76', 3),

	-- Начальник штаба
	('c0b2c93f-c639-5387-7a8d-6383c9c94317', 0),
	('c0b2c93f-c639-5387-7a8d-6383c9c94317', 1),
	('c0b2c93f-c639-5387-7a8d-6383c9c94317', 2),
	('c0b2c93f-c639-5387-7a8d-6383c9c94317', 3),

	-- Начальник оперативного отдела
	('de1c40cb-cd2b-ccf7-7c71-df2e4f08c0a0', 0),
	('de1c40cb-cd2b-ccf7-7c71-df2e4f08c0a0', 1),
	('de1c40cb-cd2b-ccf7-7c71-df2e4f08c0a0', 2),

	-- Офицер направления на вышестоящий КП
	('06b00aad-0d0c-c0a3-82e0-3fe83862c7d2', 0),
	('06b00aad-0d0c-c0a3-82e0-3fe83862c7d2', 1),
	('06b00aad-0d0c-c0a3-82e0-3fe83862c7d2', 2),

	-- Офицер направления на взаимодействующие КП
	('f06a75bc-8c8d-ad8d-0a45-61c5c07e9a09', 0),
	('f06a75bc-8c8d-ad8d-0a45-61c5c07e9a09', 1),
	('f06a75bc-8c8d-ad8d-0a45-61c5c07e9a09', 2);

SELECT * FROM role_subsystem_permission;