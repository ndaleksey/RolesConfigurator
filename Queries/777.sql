set search_path to 'permission';

select r.name from role r join account a on r.id = a.role_id where a.login = 'avdeev';
select r.name, rp.plugin_name from role r join role_plugin rp on r.id = rp.role_id order by r.name, rp.plugin_name