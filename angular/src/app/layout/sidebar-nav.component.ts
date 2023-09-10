import { Component, Injector, ViewEncapsulation } from '@angular/core';
import { AppComponentBase } from '@shared/app-component-base';
import { MenuItem } from '@shared/layout/menu-item';
import { MatDialog } from '@angular/material';
import { EditSidebarComponent } from './edit-sidebar/edit-sidebar.component';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';

@Component({
    templateUrl: './sidebar-nav.component.html',
    selector: 'sidebar-nav',
    encapsulation: ViewEncapsulation.None
})
export class SideBarNavComponent extends AppComponentBase {
    menuItems: MenuItem[];
    getMenuItems(): MenuItem[] {
        return [
            new MenuItem(this.l('My profile'), 'MyProfile', 'account_box', '/app/main/my-profile'),
            new MenuItem(this.l('Admin'), '', 'group_work', '', [
                new MenuItem(this.l('Users'), 'Admin.Users', 'people', '/app/main/user'),
                new MenuItem(this.l('Roles'), 'Admin.Roles', 'local_offer', '/app/roles'),
                new MenuItem(this.l('Configuration'), 'Admin.Configuration', 'settings_applications', '/app/configuration'),
                new MenuItem(this.l('Clients'), 'Admin.Clients', 'people_outline', '/app/main/customers'),
                new MenuItem(this.l('Tasks'), 'Admin.Tasks', 'import_contacts', '/app/main/tasks'),
                new MenuItem(this.l('Leave types'), 'Admin.LeaveTypes', 'date_range', '/app/main/manage-absence-types'),
                new MenuItem(this.l('Branches'), 'Admin.Branchs', 'apartment', '/app/main/branchs'),
                new MenuItem(this.l('Position'), 'Admin.Position', 'description', '/app/main/position'),
                new MenuItem(this.l('Capability'), 'Admin.Capability', 'view_list', '/app/main/capabilities'),
                new MenuItem(this.l('Capability setting'), 'Admin.CapabilitySetting', 'settings_accessibility', '/app/main/capability-settings'),
                new MenuItem(this.l('Off day setting'), 'DayOff', 'date_range', '/app/main/off-day'),
                new MenuItem(this.l('Overtime settings'), 'OverTimeSetting', 'access_time', '/app/main/overtime-setting'),
                new MenuItem(this.l('Audit logs'), 'Admin.AuditLog', 'miscellaneous_services', '/app/main/auditlog'),
                new MenuItem(this.l('Backgound Job'), 'Admin.BackgroundJob', 'update', '/app/main/background-jobs'),
            ]),
            // new MenuItem(this.l('Users'), 'Pages.Users', 'people', '/app/main/user'),
            // new MenuItem(this.l('Roles'), 'Pages.Roles', 'local_offer', '/app/roles'),
            // new MenuItem(this.l('Configuration'), 'Pages.EmailConfiguration', 'settings_applications', '/app/configuration'),
            new MenuItem(this.l('Projects'), 'Project', 'assessment', '/app/main/projects'),
            new MenuItem(this.l('My timesheets'), 'MyTimesheet', 'alarm', '/app/main/mytimesheets'),
            new MenuItem(this.l('My request off/remote/onsite'), 'MyAbsenceDay', 'event_busy', '/app/main/absence-day'),
            new MenuItem(this.l('My working time'), 'MyWorkingTime', 'today', '/app/main/my-working-time'),
            new MenuItem(this.l('Manage timesheet'), 'Timesheet', 'date_range', '/app/main/timesheets'),
            new MenuItem(this.l("Manage request off/remote/onsite"), "AbsenceDayByProject", 'rule', '/app/main/off-day-project'),
            new MenuItem(this.l('Manage working times'), 'ManageWorkingTime', 'access_time', '/app/main/manage-working-times'),
            new MenuItem(this.l("Team working calendar"), "AbsenceDayOfTeam", 'groups', '/app/main/off-day-project-for-user'),
            new MenuItem(this.l('Timesheets monitoring'), 'TimesheetSupervision', 'supervised_user_circle', '/app/main/timesheets-supervisior'),
            //new MenuItem(this.l('Leave days'), 'AbsenceDay', 'done_all', '/app/main/absence-request'),
            // new MenuItem(this.l('Setting off days'), 'DayOff', 'date_range', '/app/main/off-day'),
            // new MenuItem(this.l('Overtime settings'), 'OverTimeSetting', 'date_range', '/app/main/overtime-setting'),
            // manage working time
            // new MenuItem(this.l('User Salary'), 'Pages.UserSalary', 'people', '/app/main/user-salary'),
            // new MenuItem(this.l('User Salary Month'), 'Pages.UserSalaryMonth', 'monetization_on', '/app/main/user-salary-month'),
            new MenuItem(this.l('Retro'), 'Retro', 'event_note', '/app/main/retro'),
            new MenuItem(this.l('Review Interns'), 'ReviewIntern', 'rate_review', '/app/main/review'),

            new MenuItem(this.l('Report'), 'Report', 'description', '', [
                new MenuItem(this.l('Interns Info'), 'Report.InternsInfo', 'description', '/app/main/interns-info'),
                new MenuItem(this.l('Normal working'), 'Report.NormalWorking', 'work_outline', '/app/main/normal-working'),
                new MenuItem(this.l('Over time'), 'Report.OverTime', 'date_range', '/app/main/over-time'),
                new MenuItem(this.l('Tardiness'), 'Report.TardinessLeaveEarly', 'wysiwyg', '/app/main/tardiness-leave-early'),
                new MenuItem(this.l('Komu tracker'), 'Report.KomuTracker', 'addchart', '/app/main/komu-tracker'),
            ]),
            new MenuItem(this.l('Team building'), 'TeamBuilding', 'store', '', [
                new MenuItem(this.l('Team building HR'), 'TeamBuilding.DetailHR', 'supervisor_account', '/app/main/team-building-hr'),
                new MenuItem(this.l('PM request'), 'TeamBuilding.DetailPM', 'supervisor_account', '/app/main/team-building-pm'),
                new MenuItem(this.l('Request history'), 'TeamBuilding.Request', 'speaker_notes', '/app/main/team-building-request'),
                new MenuItem(this.l('Team building project'), 'TeamBuilding.Project', 'done_all', '/app/main/team-building-project'),
            ]),
        ];
    }
    ngOnInit(): void {
        this.menuItems = this.getMenuItems();
    }

    constructor(
        injector: Injector,
        private _dialog: MatDialog,
        private router: Router,
    ) {
        super(injector);
    }

    contextMenuPosition = { x: '0px', y: '0px' };
    onContextMenu(event: MouseEvent, temp) {
        event.preventDefault();
        this.contextMenuPosition.x = event.clientX + 'px';
        this.contextMenuPosition.y = event.clientY + 'px';
        temp.openMenu();
    }
    showMenuItem(menuItem): boolean {
        if (menuItem.permissionName) {
            return this.permission.isGranted(menuItem.permissionName);
        } else if (menuItem.items && menuItem.items.length > 0) {
            return menuItem.items.some(s => this.permission.isGranted(s.permissionName))
        }

        return true;
    }
}
