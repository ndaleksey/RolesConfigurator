DROP SCHEMA IF EXISTS permission CASCADE;
CREATE SCHEMA permission;

COMMENT ON SCHEMA permission IS 'Схема для разграничения прав доступа ролей и пользователей к плагинам (модулям) оболочки приложений, их функционалу, подсистемам КСА, а также для хранения кластеров подразделений';

set search_path to 'permission';

-- список подсистем
CREATE TABLE subsystem (
	number INTEGER NOT NULL,
	name TEXT NOT NULL,

	PRIMARY KEY (number)
);

COMMENT ON TABLE subsystem IS 'Таблица подсистем КСА, разрешённые для использования ролью';
COMMENT ON COLUMN subsystem.number IS 'Номер подсистемы';
COMMENT ON COLUMN subsystem.name IS 'Наименование подсистемы';

-- собственно, его величество "КЛАСТЕР" =)
CREATE TABLE cluster (
	id uuid NOT NULL,
	number INTEGER NOT NULL,
	priority INTEGER,
	
	PRIMARY KEY (id),
	UNIQUE(number),
	FOREIGN KEY (id) REFERENCES dynamic.mu_own_bb_obj(id) ON DELETE CASCADE
);

COMMENT ON TABLE cluster IS 'Таблица подсистем КСА, разрешённые для использования ролью';
COMMENT ON COLUMN cluster.id IS 'Идентификатор элемента группировки (подразделения)';
COMMENT ON COLUMN cluster.number IS 'Номер кластера';
COMMENT ON COLUMN cluster.priority IS 'Приоритет (вышестоящие/нижестоящие)';


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

COMMENT ON TABLE role IS 'Таблица ролей пользователей (внутренние абоненты)';
COMMENT ON COLUMN role.id IS 'Идентификатор';
COMMENT ON COLUMN role.name IS 'Наименование';
COMMENT ON COLUMN role.description IS 'Описание';
COMMENT ON COLUMN role.number IS 'Номер';

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

COMMENT ON TABLE account IS 'Таблица пользователей (пользователи СУБД)';
COMMENT ON COLUMN account.login IS 'Логин';
COMMENT ON COLUMN account.name IS 'Имя';
COMMENT ON COLUMN account.description IS 'Описание';
COMMENT ON COLUMN account.role_id IS 'Идентификатор роли, которой принадлежит данный пользователь';

CREATE TABLE role_plugin (
	role_id uuid NOT NULL,
	plugin_name TEXT NOT NULL,

	PRIMARY KEY (role_id, plugin_name),
	FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE CASCADE
);

COMMENT ON TABLE role_plugin IS 'Таблица связи ролей и плагинов';
COMMENT ON COLUMN role_plugin.role_id IS 'Идентификатор роли';
COMMENT ON COLUMN role_plugin.plugin_name IS 'Наименование плагина';


-- разрешения на ф-цонал плагина для роли
CREATE TABLE role_plugin_permission (
	role_id uuid NOT NULL,
	plugin_name TEXT NOT NULL,
	permission_name TEXT NOT NULL,
	permission_value SMALLINT NOT NULL,

	PRIMARY KEY (role_id, plugin_name, permission_name),
	FOREIGN KEY (role_id) REFERENCES role(id) ON DELETE CASCADE
);

COMMENT ON TABLE role_plugin_permission IS 'Таблица связи ролей и разрешений модулей (функционала)';
COMMENT ON COLUMN role_plugin_permission.role_id IS 'Идентификатор роли';
COMMENT ON COLUMN role_plugin_permission.plugin_name IS 'Наименование плагина';
COMMENT ON COLUMN role_plugin_permission.permission_name IS 'Наименование разрешения';
COMMENT ON COLUMN role_plugin_permission.permission_value IS 'Значение разрешения';

-- разрешения на тип подсистем внутренних абонентов
CREATE TABLE role_subsystem_permission (
	subsystem_number INTEGER NOT NULL,
	role_id uuid NOT NULL,
	
	PRIMARY KEY (subsystem_number, role_id),
	FOREIGN KEY (role_id) REFERENCES role(id) ON UPDATE CASCADE ON DELETE CASCADE,
	FOREIGN KEY (subsystem_number) REFERENCES subsystem(number) ON UPDATE CASCADE ON DELETE CASCADE
);

COMMENT ON TABLE role_subsystem_permission IS 'Таблица связи ролей и разрешений подсистем КСА';
COMMENT ON COLUMN role_subsystem_permission.subsystem_number IS 'Номер подсистемы КСА';
COMMENT ON COLUMN role_subsystem_permission.role_id IS 'Идентификатор роли';

-- внешние абоненты
CREATE TABLE external_abonent (
	cluster_number INTEGER NOT NULL,
	number INTEGER NOT NULL,
	name TEXT NOT NULL,
	description TEXT,
	
	PRIMARY KEY (cluster_number, number),
	UNIQUE (name)
);

COMMENT ON TABLE role_subsystem_permission IS 'Таблица внешних абонентов';
COMMENT ON COLUMN role_subsystem_permission.subsystem_number IS 'Номер подсистемы КСА';

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