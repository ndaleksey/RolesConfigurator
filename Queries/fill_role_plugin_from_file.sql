SET search_path TO 'permission';

DROP TABLE if exists plugin;

CREATE TABLE plugin (
	name text NOT NULL
);

-- копирование всех плагинов из файла с плагинами (ТУТ УКАЗАТЬ ПУТЬ!!!)
COPY plugin FROM 'd:\plugins.txt';


DROP FUNCTION if exists load_plugins_for_all_roles();

-- функция загрузки разрешения на старт модулей (из файла) для каждой роли
CREATE OR REPLACE FUNCTION load_plugins_for_all_roles() RETURNS void AS 
$$
DECLARE
	p plugin%ROWTYPE;
	r role%ROWTYPE;
BEGIN
	FOR p IN
		SELECT * FROM plugin
	LOOP
		FOR r IN
			SELECT * FROM role
		LOOP
			INSERT INTO role_plugin (role_id, plugin_name) VALUES (r.id, p.name);
		END LOOP;
	END LOOP;
END;
$$ 
LANGUAGE plpgsql;

DELETE FROM role_plugin;

SELECT * FROM load_plugins_for_all_roles();

-- проверка
SELECT * FROM role_plugin ORDER BY role_id, plugin_name;