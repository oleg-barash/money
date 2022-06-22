create table credit
(
    id           integer   not null
        constraint credit_pk
            primary key,
    description  varchar,
    sum          money     not null,
    duration     integer   not null,
    interest     numeric   not null,
    date         timestamp not null,
    paymentvalue money     not null,
    constraint actions_credit_id_fk
        foreign key (credit) references credit
);

alter table credit
    owner to postgres;

create table actions
(
    id          serial
        constraint actions_pk
            primary key,
    date        timestamp not null,
    type        integer   not null,
    value       money     not null,
    description varchar   not null,
    category    integer   not null,
    credit      integer
        constraint actions_credit_id_fk
            references credit (id)
);

alter table actions
    owner to postgres;

create unique index actions_id_uindex
    on actions (id);

create table creditpayment
(
    credit integer   not null
        constraint creditpayment_credit_id_fk
            references credit (id),
    date   timestamp not null
);

alter table creditpayment
    owner to postgres;

