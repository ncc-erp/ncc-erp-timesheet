import { Component, Injector, ViewEncapsulation } from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import { MenuItem } from "@shared/layout/menu-item";
import { MatDialog } from "@angular/material";
import { EditSidebarComponent } from "./edit-sidebar/edit-sidebar.component";
import {
    Router,
    ActivatedRoute,
    NavigationEnd,
    RouterEvent,
    PRIMARY_OUTLET,
} from "@angular/router";
import { BehaviorSubject } from "rxjs";
import { filter } from "rxjs/operators";

@Component({
    templateUrl: "./sidebar-nav.component.html",
    selector: "sidebar-nav",
    encapsulation: ViewEncapsulation.None,
})
export class SideBarNavComponent extends AppComponentBase {
    menuItems: MenuItem[];
    menuItemsMap: { [key: number]: MenuItem } = {};
    activatedMenuItems: MenuItem[] = [];
    routerEvents: BehaviorSubject<RouterEvent> = new BehaviorSubject(undefined);
    homeRoute = "/app/main/mytimesheets";
    getMenuItems(): MenuItem[] {
        return [
            new MenuItem(
                this.l("My information"),
                "MyProfile",
                "account_box",
                "/app/main/my-profile"
            ),
            new MenuItem(this.l("Admin"), "", "admin_panel_settings", "", [
                new MenuItem(
                    this.l("Users"),
                    "Admin.Users",
                    "people",
                    "/app/main/user"
                ),
                new MenuItem(
                    this.l("Roles"),
                    "Admin.Roles",
                    "local_offer",
                    "/app/roles"
                ),
                new MenuItem(
                    this.l("Configuration"),
                    "Admin.Configuration",
                    "settings_applications",
                    "/app/configuration"
                ),
                new MenuItem(
                    this.l("Clients"),
                    "Admin.Clients",
                    "people_outline",
                    "/app/main/customers"
                ),
                new MenuItem(
                    this.l("Tasks"),
                    "Admin.Tasks",
                    "import_contacts",
                    "/app/main/tasks"
                ),
                new MenuItem(
                    this.l("Leave types"),
                    "Admin.LeaveTypes",
                    "date_range",
                    "/app/main/manage-absence-types"
                ),
                new MenuItem(
                    this.l("Branches"),
                    "Admin.Branchs",
                    "apartment",
                    "/app/main/branchs"
                ),
                new MenuItem(
                    this.l("Position"),
                    "Admin.Position",
                    "description",
                    "/app/main/position"
                ),
                new MenuItem(
                    this.l("Capability"),
                    "Admin.Capability",
                    "view_list",
                    "/app/main/capabilities"
                ),
                new MenuItem(
                    this.l("Capability setting"),
                    "Admin.CapabilitySetting",
                    "settings_accessibility",
                    "/app/main/capability-settings"
                ),
                new MenuItem(
                    this.l("Off day setting"),
                    "DayOff",
                    "date_range",
                    "/app/main/off-day"
                ),
                new MenuItem(
                    this.l("Overtime settings"),
                    "OverTimeSetting",
                    "access_time",
                    "/app/main/overtime-setting"
                ),
                new MenuItem(
                    this.l("Audit logs"),
                    "Admin.AuditLog",
                    "miscellaneous_services",
                    "/app/main/auditlog"
                ),
                new MenuItem(
                    this.l("Backgound Job"),
                    "Admin.BackgroundJob",
                    "update",
                    "/app/main/background-jobs"
                ),
            ]),
            new MenuItem(this.l("Personal timesheet"), "", "account_circle", "", [
                new MenuItem(
                    this.l("My timesheet"),
                    "MyTimesheet",
                    "alarm",
                    "/app/main/mytimesheets"
                ),
                new MenuItem(
                    this.l("My off/remote/onsite requests"),
                    "MyAbsenceDay",
                    "event_busy",
                    "/app/main/absence-day"
                ),
                new MenuItem(
                    this.l("Team working calendar"),
                    "AbsenceDayOfTeam",
                    "groups",   
                    "/app/main/off-day-project-for-user"
                ),
                new MenuItem(
                    this.l("My working time"),
                    "MyWorkingTime",
                    "today",
                    "/app/main/my-working-time"
                ),
            ]),
            new MenuItem(this.l("Management"), "", "group_work", "", [
                new MenuItem(
                    this.l("Manage off/remote/onsite requests"),
                    "AbsenceDayByProject",
                    "rule",
                    "/app/main/off-day-project"
                ),
                new MenuItem(
                    this.l("Timesheet management"),
                    "Timesheet",
                    "date_range",
                    "/app/main/timesheets"
                ),
                new MenuItem(
                    this.l("Timesheets monitoring"),
                    "TimesheetSupervision",
                    "supervised_user_circle",
                    "/app/main/timesheets-supervisior"
                ),
                new MenuItem(
                    this.l("Project management"),
                    "Project",
                    "assessment",
                    "/app/main/projects"
                ),
                new MenuItem(
                    this.l("Review Interns"),
                    "ReviewIntern",
                    "rate_review",
                    "/app/main/review"
                ),
                new MenuItem(
                    this.l("Retrospectives"),
                    "Retro",
                    "event_note",
                    "/app/main/retro"
                ),
                new MenuItem(
                    this.l("Manage employee working times"),
                    "ManageWorkingTime",
                    "access_time",
                    "/app/main/manage-working-times"
                ),
                new MenuItem(
                    this.l("Branch Manager"),
                    "ProjectManagementBranchDirectors",
                    "location_city",
                    "/app/main/branch-manager"
                ),
                new MenuItem(this.l("Team building"), "TeamBuilding", "store", "", [
                    new MenuItem(
                        this.l("Team building HR"),
                        "TeamBuilding.DetailHR",
                        "supervisor_account",
                        "/app/main/team-building-hr"
                    ),
                    new MenuItem(
                        this.l("PM request"),
                        "TeamBuilding.DetailPM",
                        "supervisor_account",
                        "/app/main/team-building-pm"
                    ),
                    new MenuItem(
                        this.l("Request history"),
                        "TeamBuilding.Request",
                        "speaker_notes",
                        "/app/main/team-building-request"
                    ),
                    new MenuItem(
                        this.l("Team building project"),
                        "TeamBuilding.Project",
                        "done_all",
                        "/app/main/team-building-project"
                    ),
                ]),
                new MenuItem(this.l("Report"), "Report", "description", "", [
                    new MenuItem(
                        this.l("Interns Info"),
                        "Report.InternsInfo",
                        "description",
                        "/app/main/interns-info"
                    ),
                    new MenuItem(
                        this.l("Normal working"),
                        "Report.NormalWorking",
                        "work_outline",
                        "/app/main/normal-working"
                    ),
                    new MenuItem(
                        this.l("Over time"),
                        "Report.OverTime",
                        "date_range",
                        "/app/main/over-time"
                    ),
                    new MenuItem(
                        this.l("Tardiness"),
                        "Report.TardinessLeaveEarly",
                        "wysiwyg",
                        "/app/main/tardiness-leave-early"
                    ),
                    new MenuItem(
                        this.l("Komu tracker"),
                        "Report.KomuTracker",
                        "addchart",
                        "/app/main/komu-tracker"
                    ),
                ]),
            ]),
        ];
    }

    constructor(injector: Injector, private router: Router) {
        super(injector);
        this.router.events.subscribe(this.routerEvents);
    }

    ngOnInit(): void {
        this.menuItems = this.getMenuItems();
        this.patchMenuItems(this.menuItems);
        this.routerEvents
            .pipe(filter((event) => event instanceof NavigationEnd))
            .subscribe((event) => {
                const currentUrl = event.url !== "/" ? event.url : this.homeRoute;
                const primaryUrlSegmentGroup =
                    this.router.parseUrl(currentUrl).root.children[PRIMARY_OUTLET];
                if (primaryUrlSegmentGroup) {
                    this.activateMenuItems("/" + primaryUrlSegmentGroup.toString());
                }
            });
    }

    patchMenuItems(items: MenuItem[], parentId?: number): void {
        items.forEach((item: MenuItem, index: number) => {
            item.id = parentId ? Number(parentId + "" + (index + 1)) : index + 1;
            if (parentId) {
                item.parentId = parentId;
            }
            if (parentId || item.children) {
                this.menuItemsMap[item.id] = item;
            }
            if (item.children) {
                this.patchMenuItems(item.children, item.id);
            }
        });
    }

    activateMenuItems(url: string): void {
        this.deactivateMenuItems(this.menuItems);
        this.activatedMenuItems = [];
        const foundedItems = this.findMenuItemsByUrl(url, this.menuItems);
        foundedItems.forEach((item) => {
            this.activateMenuItem(item);
        });
    }

    deactivateMenuItems(items: MenuItem[]): void {
        items.forEach((item: MenuItem) => {
            item.isActive = false;
            item.isCollapsed = true;
            if (item.children) {
                this.deactivateMenuItems(item.children);
            }
        });
    }

    findMenuItemsByUrl(
        url: string,
        items: MenuItem[],
        foundedItems: MenuItem[] = []
    ): MenuItem[] {
        items.forEach((item: MenuItem) => {
            if (url === item.route && !item.children) {
                foundedItems.push(item);
            } else if (item.children) {
                this.findMenuItemsByUrl(url, item.children, foundedItems);
            }
        });
        return foundedItems;
    }

    activateMenuItem(item: MenuItem): void {
        item.isActive = true;
        if (item.children) {
            item.isCollapsed = false;
        }
        this.activatedMenuItems.push(item);
        if (item.parentId) {
            this.activateMenuItem(this.menuItemsMap[item.parentId]);
        }
    }

    isMenuItemVisible(item: MenuItem): boolean {    
        if (this.permission.isGranted(item.permissionName)) {
            return true;
        }

        if (item.children != null && item.children.length > 0) {
            var check = item.children.find((i) =>
                this.permission.isGranted(i.permissionName)
            );
            return check != null;
        }
        return this.permission.isGranted(item.permissionName);
    }
}
