CREATE OR REPLACE VIEW 
	permission.superior_grouping
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
		link.id = '1052fb81-3d53-44f4-aeb1-de3394a9eb31'::uuid
	ORDER BY 
		link.parent_id;
	
COMMENT ON VIEW permission.superior_grouping is 'Группировка "Вышестоящие"';