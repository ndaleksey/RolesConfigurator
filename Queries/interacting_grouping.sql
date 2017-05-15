CREATE OR REPLACE VIEW 
	permission.interacting_grouping
AS 
	SELECT 
		military_unit.id,
		link.parent_id,
		military_unit.name,
		military_unit.frm_type,
		military_unit.location
	FROM 
		dynamic.sys_tree_link link
	LEFT JOIN 
		dynamic.military_unit military_unit 
	ON 
		link.obj_id = military_unit.id
	WHERE 
		link.id = '8c2f0e2e-a0f9-46ef-9206-b59eb9587b89'::uuid
	ORDER BY 
		link.parent_id;

	COMMENT ON VIEW permission.interacting_grouping is 'Группировка "Взаимодействующие"';