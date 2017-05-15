CREATE OR REPLACE VIEW 
	permission.inferior_grouping
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
		link.id = '73defe82-a6a0-4fd1-a160-20ae6a8a1381'::uuid
	ORDER BY 
		link.parent_id;

	COMMENT ON VIEW permission.inferior_grouping is 'Группировка "Нижестоящие"';