select 
	r.id as role_id, 
	r.name as role_name, 
	r.description as role_description, 
	r.number as role_number, 
	s.number as subsystem_number, 
	s.name as subsystem_name 
from 
	permission.role r 
join 
	permission.role_subsystem_permission rs 
on 
	r.id = rs.role_id 
join 
	permission.subsystem s 
on 
	s.number = rs.subsystem_number
