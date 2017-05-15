SET search_path TO 'permission';

DELETE FROM subsystem;

INSERT INTO 
	subsystem 
VALUES 
	(0, 'КСТ'),
	(1, 'ЭП'),
	(2, 'БГ'),
	(3, 'Сервер репликаций БД'),
	(4, 'Надводная обстановка'),
	(5, 'Учёт личного состава');

SELECT * FROM subsystem;