
/*
This part of the code is created by Syed Muhammad Saim: Intern.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Threading;
using WinForms = System.Windows.Forms;

using Ranorex;
using Ranorex.Core;
using Ranorex.Core.Repository;
using Ranorex.Core.Testing;

namespace FlexconfigRBS_GUI.RM.DeviceConfig.SelectDatabase
{
    public partial class A_Location_DB
    {
        /// <summary>
        /// This method gets called right after the recording has been started.
        /// It can be used to execute recording specific initialization code.
        /// </summary>
        private void Init()
        {
        	//_varLocationDB = @"C:\SVN_DB_Rep\FIBEX\FR_CAN_4All\FR_CAN_4All__FIBEX3.1.xml";
        	Button btnOk = "/form[@controlname='DatabaseManagerDialog']/?/?/button[@controlname='btnOk']";
        	List list = "/form[@controlname='DatabaseManagerDialog']/container[@controlname='pnlContent']/container[@controlname='pnlLeft']/container[@controlname='databasesOverviewControl']/table[@controlname='olvDatabases']/list[@accessiblerole='List']";
        	ProgressBar progressBar = "/form[@controlname='DatabaseManagerDialog']/?/?/progressbar[@controlname='progressBar']";
        	Text lblProgressText = "/form[@controlname='DatabaseManagerDialog']/?/?/text[@controlname='lblProgressText']";
        	
        	int rowCount = list.Items.Count;
        	string[] location = _varLocationDB.Split('\\');
        	
        	int len = location.Length;
        	string dbName = location[len - 1];
        	
        	bool found = false;
        	bool ecu_dialog_visible = false;
        	bool failFlag = false;
        	int counter = 0;
        	
        	// Remove quotation mark at beginning or end of dbName, if it exists
        	//------------------------------------------------------------------
        	if(dbName[0] == '"')
        		dbName = dbName.Remove(0,1);
        	
        	if(dbName[dbName.Length - 1] == '"')
        		dbName = dbName.Remove(dbName.Length - 1, 1);
        		
        	
        	// Check if database is inside list, if it does, click on it and click on ok
        	//--------------------------------------------------------------------------
        	for(int i = 0; i < rowCount; i++)
        	{
        		var row = list.Items[i];
        		//Report.Info(row.Items.Count.ToString());
        		//Report.Info(list.Items.Count.ToString());
        		Delay.Milliseconds(500);
        		if(list.Items[i].Text.Contains(dbName))
        		{
        			list.Items[i].Click();
        			btnOk.Click();
        			Report.Success("Database found and selected");
        			found = true;
        			break;
        		}
        	}
        	
        	
        	// If database is not inside the list, add it, click on it and click on ok
        	//------------------------------------------------------------------------
        	if(found == false)
        	{
    			Button buttonAdd = "/form[@controlname='DatabaseManagerDialog']/?/?/toolbar[@controlname='headerStrip']/button[@accessiblename='Add']";
    			buttonAdd.Click();
    			
    			Text text1148 = "/form[@title='Select file']/?/?/text[@controlid='1148']";
    			text1148.Click();
    			Keyboard.Press(_varLocationDB);
    			
    			Delay.Milliseconds(1000);
    			
    			Button oEffnen = "/form[@title='Select file']/button[@text='Ö&ffnen']";
    			oEffnen.Click();

	            Delay.Milliseconds(5000);
	            
	            // Wait until database is loaded, it is loading during the while-loop
	            //-------------------------------------------------------------------
	            while(btnOk.Enabled == false)
	            {
	            	// After the database finishes loading, there are some cases where the RBS is hanging/frozen
	            	// This is noticeable from some parts of the dialog being blackened
	            	// When this happens, the objects inside the window are not accessible,
	            	// so we need to catch this or otherwise the test is failing
	            	//----------------------------------------------------------
	            	try
	            	{
		            	// Get current state of progressBar
		            	//---------------------------------
		            	progressBar = "/form[@controlname='DatabaseManagerDialog']/?/?/progressbar[@controlname='progressBar']";
		            	lblProgressText = "/form[@controlname='DatabaseManagerDialog']/?/?/text[@controlname='lblProgressText']";
		            	btnOk = "/form[@controlname='DatabaseManagerDialog']/?/?/button[@controlname='btnOk']";
		            	Report.Info("Progressbar value: " + progressBar.Value.ToString());
		            	Report.Info("Progressbar label: " + lblProgressText.TextValue);
		            	Report.Info("ok-Button value: " + btnOk.Enabled.ToString());
		            	
		            	// Break condition 1: (failure)
		            	// When the progressBar and btnOk is not active/enabled, something went wrong,
		            	// so we have to stop the test at this point
		            	//------------------------------------------
		            	if(progressBar.Value == 0 && btnOk.Enabled == false && lblProgressText.TextValue != "Initialize...")
		            	{
		            		Report.Failure("Failed to load the database. ProgressBar AND okButton was inactive.");
		            		failFlag = true;
		            		break;
		            	}
		            	
		            	// Break condition 2: (ok)
		            	// Check if the ECU Families dialog appears, if it does, exit the loop
		            	//--------------------------------------------------------------------
		            	try
		            	{
		            		Form selectFamilyConverterDialog = "/form[@controlname='SelectFamilyConverterDialog']";
		            		ecu_dialog_visible = selectFamilyConverterDialog.Element.Visible;
		            	}
		            	catch
		            	{
		            		// The try-catch blocks the execution for roughly 4s when we checked it (due to exception)
		            		// so we have to take this into account for the timeout
		            		//-----------------------------------------------------
		            		counter += 4;
		            		ecu_dialog_visible = false;
		            	}
		            	if(ecu_dialog_visible)
		            		break;
		            	
		            	
		            	// Break condition 3: (failure)
		            	// Check, if more than 2h passed, cancel waiting
		            	//------------------------------------------------------------------
		            	if(counter > 7200)
		            	{
		            		Report.Failure("Test failed due to database loading too long or the Ok-Button not being clickable (disabled).");
		            		failFlag = true;
		            		break;
		            	}
		            	
		            	Delay.Milliseconds(1000);
		            	counter++;
		            }
	            	catch
	            	{
	            		// The try-catch blocks the execution for roughly 4s when we checked it (due to exception)
	            		// so we have to take this into account for the timeout
	            		//-----------------------------------------------------
	            		counter += 4;
	            	}
	            }
	            
	            // Skip the selection of database, after loading, if the "ECU Family Dialog" is blocking it
	            // The selection will be handled in the "A_Btn_OK_ECUFamilyHandlings.UserCode.cs"
	            //-------------------------------------------------------------------------------
	            if(!ecu_dialog_visible)
	            {
	            	// After adding it, loop through the database items and click on the one that we need to add, then click ok
		            //---------------------------------------------------------------------------------------------------------
		            if(!failFlag)
		            {
		            	list = "/form[@controlname='DatabaseManagerDialog']/container[@controlname='pnlContent']/container[@controlname='pnlLeft']/container[@controlname='databasesOverviewControl']/table[@controlname='olvDatabases']/list[@accessiblerole='List']";
		            	rowCount = list.Items.Count;
		            	
		            	for(int i = 0; i < rowCount; i++)
		            	{
			        		var row = list.Items[i];
			        		//Report.Info(row.Items.Count.ToString());
			        		//Report.Info(list.Items.Count.ToString());
			        		Delay.Milliseconds(500);
			        		if(list.Items[i].Text.Contains(dbName))
			        		{
			        			list.Items[i].Click();
			        			Report.Success("Database found and selected");
			        			found = true;
			        			break;
			        		}
		            	}
		            	btnOk.Click();
		            }
	            }
        	}
        }
    }
}