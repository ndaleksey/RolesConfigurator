DROP SCHEMA IF EXISTS permission CASCADE;
CREATE SCHEMA permission;

set search_path to 'permission';

-- список подсистем
CREATE TABLE subsystem (
	number INTEGER NOT NULL,
	name TEXT NOT NULL,

	PRIMARY KEY (number)
);

-- таблица кластеров
CREATE TABLE cluster (
	id uuid NOT NULL,
	number INTEGER NOT NULL,
	priority INTEGER,
	
	PRIMARY KEY (id),
	UNIQUE(number),	
	FOREIGN KEY (id) REFERENCES dynamic.mu_own_bb_obj(id) ON DELETE CASCADE
);

COMMENT ON COLUMN cluster.id is 'ID элемента группировки';

CREATE TABLE role (
	id uuid NOT NULL,
	cluster_id uuid NOT NULL,
	number INTEGER NOT NULL,
	name TEXT NOT NULL,
	description TEXT,
	
	PRIMARY KEY (id),
	UNIQUE (cluster_id, number),
	UNIQUE(cluster_id, name)
);
COMMENT ON COLUMN cluster.id is 'Таблица внутренних и внешних ролей (абонентов)';

-- пользователи (БД или в будущем домена Windows)
CREATE TABLE account (
	login TEXT NOT NULL,
	name TEXT NOT NULL,
	description TEXT,
	role_id uuid,

	PRIMARY KEY (login),
	UNIQUE (name),
	FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE SET NULL
);

-- связь между плагином и ролью
CREATE TABLE role_plugin (
	role_id uuid NOT NULL,
	plugin_name TEXT NOT NULL,

	PRIMARY KEY (role_id, plugin_name),
	FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE CASCADE
);

-- разрешения на ф-цонал плагина для роли
CREATE TABLE role_plugin_permission (
	role_id uuid NOT NULL,
	plugin_name TEXT NOT NULL,
	permission_name TEXT NOT NULL,
	permission_value SMALLINT NOT NULL,

	PRIMARY KEY (role_id, plugin_name, permission_name),
	FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE CASCADE
);

-- разрешения на тип подсистем ролей
CREATE TABLE role_subsystem_permission (
	subsystem_number INTEGER NOT NULL,
	role_id uuid NOT NULL,
	
	PRIMARY KEY (subsystem_number, role_id),
	FOREIGN KEY (role_id) REFERENCES role(id) ON UPDATE CASCADE ON DELETE CASCADE,
	FOREIGN KEY (subsystem_number) REFERENCES subsystem(number) ON UPDATE CASCADE ON DELETE CASCADE
);

COMMENT ON COLUMN role_subsystem_permission.subsystem_number IS 'Номер подсистемы в таблице subsystem';