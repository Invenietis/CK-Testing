create schema Prd;
Go
create table Prd.tUser
(
    UserId int identity(0,1),
    FirstName nvarchar(64) not null,
    LastName nvarchar(64) not null,
    Email nvarchar(256) collate Latin1_General_CI_AI not null 
    constraint PK_Prd_tUser primary key (UserId),
    constraint UK_Prd_tUser_Email unique (Email)
);
insert into Prd.tUser(FirstName,LastName,Email) values( N'', N'', N'' );
GO
create table Prd.tCustomer
(
    CustomerId int not null,
    IsPreferential bit not null,
    constraint PK_Prd_tCustomer primary key (CustomerId),
    constraint FK_Prd_tCustomer_tUser_CustomerId foreign key (CustomerId) references Prd.tUser(UserId)
);
insert into Prd.tCustomer( CustomerId, IsPreferential ) values( 0, 0 );
GO
create table Prd.tEmployee
(
    EmployeeId int not null,
    SSN varchar(36) not null,
    constraint PK_Prd_tEmployee primary key (EmployeeId),
    constraint FK_Prd_tEmployee_tUser_EmployeeId foreign key (EmployeeId) references Prd.tUser(UserId)
);
insert into Prd.tEmployee( EmployeeId, SSN ) values( 0, '000000000000000000000000000000000000' );
GO
create table Prd.tProduct
(
    ProductId int identity(0,1),
    ProductName nvarchar(128) not null,
    Price int not null,
    ProductManagerId int not null,
    constraint PK_Prd_tProduct primary key (ProductId),
    constraint UK_Prd_tProduct_ProductName unique (ProductName),
    constraint FK_Prd_tProduct_tEmployee_ProductManagerId foreign key (ProductManagerId) references Prd.tEmployee(EmployeeId)

);
insert into Prd.tProduct(ProductName,Price,ProductManagerId) values( N'', 0, 0 );
GO
create table Prd.tOrder
(
    OrderId int identity(0,1),
    CreationDate datetime2(3) not null constraint DF_Prd_tOrder_Creationdate default sysutcdatetime(),
    OrderState char(1) not null,
    CustomerId int not null,
    constraint PK_Prd_tOrder primary key (OrderId),
    constraint FK_Prd_tOrder_tCustomer_CustomerId foreign key (CustomerId) references Prd.tCustomer(CustomerId),
    
);
insert into Prd.tOrder(OrderState,CustomerId) values( ' ', 0 );
GO
create table Prd.tOrderLine
(
    OrderId int not null,
    ProductId int not null,
    Quantity int not null,
    constraint PK_Prd_tOrderLine primary key (OrderId,ProductId),
    constraint FK_Prd_tOrderLine_tOrder_OrderId foreign key (OrderId) references Prd.tOrder(OrderId),
    constraint FK_Prd_tOrderLine_tOrder_ProductId foreign key (ProductId) references Prd.tProduct(ProductId)
);

