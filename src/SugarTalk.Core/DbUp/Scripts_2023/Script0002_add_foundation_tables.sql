create table if not exists rm_us_location
(
    id varchar(36) not null
    primary key,
    location_code varchar(50) null,
    warehouse_code varchar(10) null,
    name varchar(200) null,
    type int null,
    status int null
) charset=utf8mb4;

create table if not exists rm_position
(
    id varchar(36) not null
    primary key,
    unit_id varchar(36) null,
    name varchar(50) null,
    description varchar(150) null,
    country_code int null,
    is_active bit null
) charset=utf8mb4;

create table if not exists rm_staff
(
    id varchar(36) not null
    primary key,
    userid varchar(36) null,
    user_name varchar(50) null,
    name_cn_long varchar(150) null,
    name_en_long varchar(150) null,
    company_id varchar(36) null,
    company_name varchar(50) null,
    department_id varchar(36) null,
    department_name varchar(50) null,
    group_id varchar(36) null,
    group_name varchar(50) null,
    position_id varchar(36) null,
    position_name varchar(50) null,
    position_cn_status int null,
    position_us_status int null,
    location_id varchar(36) null,
    location_name varchar(50) null,
    superior_id varchar(36) null,
    phone_number varchar(20) null,
    email varchar(50) null,
    work_place varchar(50) null,
    country_code int null
) charset=utf8mb4;

create table if not exists rm_unit
(
    id varchar(36) not null
    primary key,
    name varchar(50) null,
    type_id int null,
    parent_id varchar(36) null,
    leader_id varchar(36) null,
    leader_country_code int null,
    location_code varchar(16000) null,
    description varchar(200) null,
    country_code int null,
    is_active bit null
) charset=utf8mb4;

