set search_path to 'permission';

DO LANGUAGE plpgsql 
$$
DECLARE
	mu_id constant uuid := 'e4739426-3619-4598-b7d7-404d02786e17';
	cluster_number constant int := 0;
	new_role_id constant uuid := 'c7cd8e29-e3f2-4e07-92d0-156c6e4db0b2';
	account_name constant text := 'admin';
	name constant text := 'Администратор';
	description constant text := 'Администратор системы';
begin
	--CREATE role admin WITH superuser login UNENCRYPTED password 'rjynbytyn' valid until 'infinity'; -- не создавать, если есть

	delete from cluster where id = mu_id;
	insert into cluster (id, number) values (mu_id, cluster_number);

	delete from role where id = new_role_id;
	insert into role (id, cluster_id, number, name, description) values (new_role_id, mu_id, 0, name, description);

	delete from account where role_id = new_role_id;
	delete from account where login = account_name;
	insert into account (login, name, description, role_id) values (account_name, name, description, new_role_id);
end
$$;