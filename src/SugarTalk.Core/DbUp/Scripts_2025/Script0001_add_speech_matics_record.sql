create table if not exists `speech_matics_record`
(
    `id` varchar(36) auto_increment primary key,
    `status` int not null,
    `meeting_number` varchar(48) not null,
    `meeting_record_id` varchar(36) not null,
    `transcription_job_id` varchar(255) not null,
    `created_by` int not null,
    `created_date` datetime(3) not null,
)charset=utf8mb4;