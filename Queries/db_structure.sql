DROP SCHEMA IF EXISTS permission CASCADE;
CREATE SCHEMA permission;

set search_path to 'permission';

-- список подсистем
CREATE TABLE subsystem (
	number INTEGER NOT NULL,
	name TEXT NOT NULL,

	PRIMARY KEY (number)
);

-- собственно, его величество "КЛАСТЕР" =)
CREATE TABLE cluster (
	id uuid NOT NULL,
	number INTEGER NOT NULL,
	priority INTEGER,
	
	PRIMARY KEY (id),
	UNIQUE(number),
	FOREIGN KEY (id) REFERENCES dynamic.mu_own_bb_obj(id) ON DELETE CASCADE
);

COMMENT ON COLUMN cluster.id is 'ID элемента группировки';

-- роли пользователей (внутр. абонент)
CREATE TABLE role (
	id uuid NOT NULL,
	name TEXT NOT NULL,
	description TEXT,
	number INTEGER NOT NULL,

	PRIMARY KEY (id),
	UNIQUE (name) DEFERRABLE INITIALLY DEFERRED,
	UNIQUE (number)
);

COMMENT ON COLUMN role.number is 'Номер абонента';

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

-- разрешения на тип подсистем внутренних абонентов
CREATE TABLE role_subsystem_permission (
	subsystem_number INTEGER NOT NULL,
	role_id uuid NOT NULL,
	
	PRIMARY KEY (subsystem_number, role_id),
	FOREIGN KEY (role_id) REFERENCES role(id) ON UPDATE CASCADE ON DELETE CASCADE,
	FOREIGN KEY (subsystem_number) REFERENCES subsystem(number) ON UPDATE CASCADE ON DELETE CASCADE
);

-- внешние абоненты
CREATE TABLE external_abonent (
	cluster_number INTEGER NOT NULL,
	number INTEGER NOT NULL,
	name TEXT NOT NULL,
	description TEXT,
	
	PRIMARY KEY (cluster_number, number),
	UNIQUE (name)
);

-- разрешения на тип подсистем внешних абонентов
CREATE TABLE external_abonent_subsystem_permission (
	cluster_number INTEGER NOT NULL,
	number INTEGER NOT NULL,
	subsystem_number INTEGER NOT NULL,
	
	PRIMARY KEY (cluster_number, number, subsystem_number),
	FOREIGN KEY (cluster_number, number) REFERENCES external_abonent(cluster_number, number) ON UPDATE CASCADE ON DELETE CASCADE,
	FOREIGN KEY (subsystem_number) REFERENCES subsystem(number) ON UPDATE CASCADE ON DELETE CASCADE
);

COMMENT ON COLUMN external_abonent_subsystem_permission.number IS 'Номер подсистемы в таблице subsystem';