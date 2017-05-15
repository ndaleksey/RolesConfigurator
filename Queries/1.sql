set search_path to 'permission';

--delete from role_plugin;
--select r.number, r.name, rp.plugin_name from role r join role_plugin rp on r.id = rp.role_id
/*update permission.role_plugin_permission set
permission_value = 1
where role_id = '8493f771-9483-49f6-84f4-145f9919f238' and plugin_name = 'ZrvInteraction' and permission_name = 'Отображение ВО на карте';*/

select r.number, r.name, rpp.plugin_name, rpp.permission_name, rpp.permission_value, rpp.* from role_plugin_permission rpp join role r on rpp.role_id = r.id order by r.name, rpp.plugin_name, rpp.permission_name
--select r.name, a.login, a.name from role r join account a on r.id = a.role_id order by r.name, a.login
--select r.number, r.name, s.name from role r join role_subsystem_permission rsp on r.id = rsp.role_id join subsystem s on s.number = rsp.subsystem_number order by r.name, s.number