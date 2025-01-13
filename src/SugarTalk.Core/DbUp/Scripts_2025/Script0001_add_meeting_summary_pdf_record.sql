create table if not exists `meeting_summary_pdf_record`
(
    `id` int auto_increment primary key,
    `summary_id` int not null,
    `target_language` int not null,
    `pdf_url` varchar(150) not null,
    `created_by` int not null,
    `created_date` datetime(3) not null
)charset=utf8mb4;
