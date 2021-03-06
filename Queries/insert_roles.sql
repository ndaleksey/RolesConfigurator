﻿SET search_path to 'permission';

delete from cluster;

insert into cluster values 
	('2fe96078-0a19-4b75-86d2-e0c909a23568', 1), -- кластер "2Д ПВО"
	('7c7b8deb-2c39-4d02-91ac-74fcd549c763', 2); -- кластер "ЦКП ПВО"

--SELECT * FROM cluster;

delete from role;

INSERT INTO role 
	(id, cluster_id, name, number)
VALUES 
	-- кластер "2Д ПВО"
	('50622c4e-7d18-298c-7b8c-c2530e83aa76', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Командир', 1),
	('c0b2c93f-c639-5387-7a8d-6383c9c94317', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Начальник штаба', 2),
	('de1c40cb-cd2b-ccf7-7c71-df2e4f08c0a0', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Начальник оперативного отдела', 3),
	('06b00aad-0d0c-c0a3-82e0-3fe83862c7d2', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на вышестоящий КП', 4),
	('f06a75bc-8c8d-ad8d-0a45-61c5c07e9a09', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на взаимодействующие КП', 5),
	('543cbdfe-ec11-4296-a2c2-3e0f3cdbc7c4', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Заместитель командира по зенитным ракетным войскам', 6),
	('d2953b32-f5b8-431d-9a4f-75de38800cca', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на систему C4I', 7),
	('87290a18-3da5-45ae-982d-1c8e96096f43', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Заместитель командира по радиотехническим войскам', 8),
	('98abee7c-f3d9-4e69-88ea-38166283354d', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на зенитные ракетные части и подразделения', 9),
	('8493f771-9483-49f6-84f4-145f9919f238', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на КП ВМБ', 11),
	('39fa2f6e-22c8-4eef-b30e-00b726174a22', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на авиационные части и подразделения (база №1)', 12),
	('27e2620e-2ff1-4ecb-b033-0c88686fd4b6', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на авиационные части и подразделения (база №2)', 13),
	('2704f3ac-4262-4aa8-87dd-f447c03566c6', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер контроля полетов и перелетов военной авиации', 14),
	('f764cae6-c318-430e-b2cb-8dac12c1888a', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Начальник разведывательного информационного центра', 15),
	('9c0a2b2b-d6a4-4217-9859-ab35604a7fdf', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер разведывательного информационного центра', 16),
	('6ab5daf3-37c2-470a-b409-5bd0e4251232', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер направления на части и подразделения радиоэлектронной борьбы', 19),
	('d8fe56e0-2950-4adc-851f-26b7fc5a93a8', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер по обработке метоинформации', 20),
	('47e66619-260b-49a8-bb72-9f806768c5a9', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Начальник разведки', 21),
	('d4b83894-e3f4-4f35-9c27-032ef142c701', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер контроля использования воздушного пространства', 22),
	('220ea2c3-6751-44e9-95a0-9ba61034213d', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Дежурный офицер по эксплуатации КСА', 23),
	('1b8c2112-5980-4eb5-a4b0-575c61ba0827', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер по документированию/воспроизведению', 24),
	('d2baa13b-2d87-4a15-a1a9-9876312abc8b', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Администратор безопасности', 25),
	('0696921e-5357-42b7-a89b-ef87e8e00dd4', '2fe96078-0a19-4b75-86d2-e0c909a23568', 'Офицер дежурный по связи', 26),

	-- кластер "ЦКП ПВО"
	('fb478344-1ded-409f-b9a9-793d0d355c28', '7c7b8deb-2c39-4d02-91ac-74fcd549c763', 'Оперативный дежурный', 1),
	('94119269-e701-48cd-a4b2-b7de3e51d1d1', '7c7b8deb-2c39-4d02-91ac-74fcd549c763', 'Офицер направленец зоны ПВО', 2),
	('79c2bd75-4682-4c54-9bf8-bf459c82c7ed', '7c7b8deb-2c39-4d02-91ac-74fcd549c763', 'Офицер оперативного управления', 3),
	('748fc53c-ec08-4b11-8ffe-5d69a53f56e8', '7c7b8deb-2c39-4d02-91ac-74fcd549c763', 'Офицер по выдачи информации на СКП', 4),
	('fbbce560-72c0-44f2-b4c9-d85d21e55d6c', '7c7b8deb-2c39-4d02-91ac-74fcd549c763', 'Офицер по эксплуатации технических средств', 5)
	
--SELECT * FROM role;
/*DELETE FROM account;

INSERT INTO account VALUES
	('vasya', 'vasya', 'Unknown', 'c0b2c93f-c639-5387-7a8d-6383c9c94317'),
	('ivanov', 'Иванов', 'л-т Иванов', '50622c4e-7d18-298c-7b8c-c2530e83aa76'),
	('pertrov', 'Петров', 'л-т Петров', '50622c4e-7d18-298c-7b8c-c2530e83aa76'),
	('sidorov', 'Сидоров', 'л-т Сидоров', '50622c4e-7d18-298c-7b8c-c2530e83aa76'),
	('stepanov', 'Степанов', 'к-н Степанов', 'c0b2c93f-c639-5387-7a8d-6383c9c94317'),
	('voroshilov', 'Ворошилов', 'к-н Ворошилов', 'c0b2c93f-c639-5387-7a8d-6383c9c94317'),
	('samoylov', 'Самойлов', 'к-н Самойлов', 'c0b2c93f-c639-5387-7a8d-6383c9c94317');*/

--select * from account;

/*SELECT 
	r.id,
	r.name AS role_name,  
	r.description AS role_description,
	a.login AS account_login,
	a.name AS account_name,
	a.description AS account_description
FROM role r JOIN account a ON a.role_id = r.id*/

