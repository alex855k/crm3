
	DROP TABLE IF EXISTS #ApplicationControllersTemp


	CREATE TABLE #ApplicationControllersTemp
	(
		[Id] [int] NOT NULL PRIMARY KEY,
		[ControllerName] [nvarchar](max) NULL,
		[ActionName] [nvarchar](max) NULL,
		[DisplayName] [nvarchar](max) NULL,
		[IsDisabled] [bit]  NULL,
		[IsPartialPage] [bit]  NULL,
		[ApplicationControllerCategoryId] [int] NULL,
	)



	--[ControllerName],[ActionName],[DisplayName],[IsDisabled],[IsPartialPage],[ApplicationControllerCategoryId]
	INSERT INTO #ApplicationControllersTemp values (1, 'Customers', 'Index', 'Customers Main Page', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (2, 'Customers', 'Create', 'Create Customer', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (3, 'Customers', 'Edit', 'Edit Customer', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (4, 'Customers', 'CustomerContacts', 'Customers Contacts', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (5, 'Customers', 'CustomerNotes', 'Customers Notes', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (6, 'Customers', 'CustomerAppointments', 'Customers Appointments', 0, 1, 1)

	INSERT INTO #ApplicationControllersTemp values (7, 'Calendar' , 'Index', 'Calendar Main Page', 0, 0, 2)
	INSERT INTO #ApplicationControllersTemp values (8, 'Budget'   , 'Index', 'Create Budget for Salesman', 0, 0, 3)
	INSERT INTO #ApplicationControllersTemp values (9, 'DailyReport', 'Index', 'Daily Report', 0, 0, 4)
	INSERT INTO #ApplicationControllersTemp values (10, 'Customers', 'CustomerOrders', 'Customers Orders', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (11, 'DailyReport', 'DailyReportUserSelection', 'Daily Report User Selection', 0, 0, 4)
	INSERT INTO #ApplicationControllersTemp values (12, 'Customers', 'SetCustomerInactive', 'Delete Customer', 0, 0, 1)
    INSERT INTO #ApplicationControllersTemp values (13, 'UploadCustomers', 'Index', 'Upload Customers', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (14, 'DashboardLists', 'Index', 'Dashboard Lists', 0, 1, 5)
	INSERT INTO #ApplicationControllersTemp values (15, 'Customers', 'CustomerTableFilterComparison', 'Customers Main Table Filter Comparison Operators', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (16, 'CRMFeatures', 'TopMenuNotification', 'Top Menu Notification Center', 0, 0, 6)
	INSERT INTO #ApplicationControllersTemp values (17, 'CRMFeatures', 'TopMenuSearch', 'Top Menu Search', 0, 0, 6)
	INSERT INTO #ApplicationControllersTemp values (18, 'CRMFeatures', 'TopMenuLanguage', 'Top Menu Language', 0, 0, 6)	
	INSERT INTO #ApplicationControllersTemp values (19, 'CompanyProfile', 'Index', 'Company Profile', 0, 0, 7)	
	INSERT INTO #ApplicationControllersTemp values (20, 'Orders', 'CustomerOrderHistory', 'Orders', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (21, 'EmailMessages', 'CustomerEmailCorrespondenceIndex', 'Email Messages', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (22, 'EmailAccounts', 'Index', 'Email Account Management', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (23, 'Orders', 'Create', 'Create New Order', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (24, 'Customers', 'CustomerNoteOptions', 'Customer Notes {Report,type,demo} options', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (25, 'Customers', 'CustomerCase', 'Customer Case', 0, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (26, 'Customers', 'CustomerTableAndOr', 'Customers Table AndOr', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (27, '', '', 'Load Dashboard Customers by User', 0, 0, 6)
	INSERT INTO #ApplicationControllersTemp values (28, 'CustomerCases', 'Index', 'Cases', 0, 0, 1)
	INSERT INTO #ApplicationControllersTemp values (30, '', 'SendSms', 'Sms Send end of day sms', 0, 0, 8)
	INSERT INTO #ApplicationControllersTemp values (32, 'CustomerCases', '', 'CustomerCases AllAccess', 0, 0, 9)
	--INSERT INTO #ApplicationControllersTemp values (33, 'CustomerCases', 'CasesForWeek', 'For a particluar week get all cases', 0, 0, 9)
	--INSERT INTO #ApplicationControllersTemp values (34, 'CustomerCases', 'CurrentWeek', 'For the current week get all cases', 0, 0, 9)
	INSERT INTO #ApplicationControllersTemp values (35, 'TimeRegistration', 'TimeRegsForWeek', 'For the current week get summarized time-registrations for every user', 0, 0, 9)
	--INSERT INTO #ApplicationControllersTemp values (36, 'CustomerCases', 'Edit', 'Move Cases To Week', 1, 1, 1)
	INSERT INTO #ApplicationControllersTemp values (37, 'Statistics',  'Index', 'Time Registration Statistics', 0, 0, 8)
	INSERT INTO #ApplicationControllersTemp values (38, 'CustomerCases',  'Edit', 'Customer cases edit', 0, 0, 9)
	INSERT INTO #ApplicationControllersTemp values (39, 'CustomerCases',  'Create', 'Customer cases create', 0, 0, 9)
	INSERT INTO #ApplicationControllersTemp values (40, 'Home',  'TimeRegWidget', 'Time Registration Widget', 0, 0, 8)
	
	
	DECLARE @AC_TableId AS INT;
	DECLARE @AC_HasIdentity AS BIT;
	SELECT @AC_TableId = OBJECT_ID('dbo.ApplicationControllers');
	SELECT @AC_HasIdentity = (OBJECTPROPERTY(@AC_TableId, 'TableHasIdentity'))

	IF (@AC_HasIdentity = 1)
		SET IDENTITY_INSERT ApplicationControllers ON
		



	MERGE INTO ApplicationControllers AS ApplicationControllersTarget
	USING #ApplicationControllersTemp AS ApplicationControllersSource
	ON ApplicationControllersTarget.Id = ApplicationControllersSource.Id
	WHEN MATCHED THEN
			UPDATE  SET  ApplicationControllersTarget.ControllerName = ApplicationControllersSource.ControllerName,
						 ApplicationControllersTarget.ActionName = ApplicationControllersSource.ActionName,
						 ApplicationControllersTarget.DisplayName = ApplicationControllersSource.DisplayName,
						 ApplicationControllersTarget.IsPartialPage = ApplicationControllersSource.IsPartialPage

	WHEN NOT MATCHED BY TARGET THEN 
					INSERT (Id,ControllerName,ActionName,DisplayName,IsDisabled,IsPartialPage,ApplicationControllerCategoryId)
					values (ApplicationControllersSource.Id,ApplicationControllersSource.ControllerName,ApplicationControllersSource.ActionName,
					ApplicationControllersSource.DisplayName,ApplicationControllersSource.IsDisabled,ApplicationControllersSource.IsPartialPage,
					ApplicationControllersSource.ApplicationControllerCategoryId);

	IF (@AC_HasIdentity = 1)
		SET IDENTITY_INSERT ApplicationControllers OFF