import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { HomeComponent } from "@app/home/home.component";
import { AppRouteGuard } from "@shared/auth/auth-route-guard";
import { MainComponent } from "./main.component";

const routes: Routes = [
  {
    path: "projects",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/project-manager/project-manager.module#ProjectManagerModule",
            data: {
              permission: "Project",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "customers",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/customer/customer.module#CustomerModule",
            data: {
              permission: "Admin.Clients",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "tasks",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/task/task.module#TaskModule",
            data: {
              permission: "Admin.Tasks",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "timesheets",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/timesheet/timesheet.module#TimesheetModule",
            data: {
              permission: "Timesheet",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "mytimesheets",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/mytimesheet/mytimesheets.module#MyTimeSheetsModule",
            data: {
              permission: "MyTimesheet",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "timesheets-supervisior",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/timesheets-supervisior/timesheets-supervisior.module#TimesheetsSupervisiorModule",
            data: {
              permission: "TimesheetSupervision",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  // {
  //     path: 'managenews',
  //     component: MainComponent,
  //     canActivate: [AppRouteGuard],
  //     children: [{
  //         path: '',
  //         children: [
  //             {
  //                 path: '',
  //                 loadChildren: '../modules/manage-news/manage-news.module#ManageNewsModule',
  //                 data: {
  //                     permission: 'Pages.NewsManagement',
  //                     preload: true
  //                 },
  //                 canActivate: [AppRouteGuard],
  //             }
  //         ]
  //     }]
  // },
  //
  // {
  //     path: 'news',
  //     component: MainComponent,
  //     canActivate: [AppRouteGuard],
  //     children: [{
  //         path: '',
  //         children: [
  //             {
  //                 path: '',
  //                 loadChildren: '../modules/news/news.module#NewsModule',
  //                 data: {
  //                     permission: 'Pages.NewsView',
  //                     preload: true
  //                 }
  //             }
  //         ]
  //     }]
  // },
  // {
  //     path: 'checkboard',
  //     component: MainComponent,
  //     canActivate: [AppRouteGuard],
  //     children: [{
  //         path: '',
  //         children: [{
  //             path: '',
  //             loadChildren: '../modules/check-board/check-board.module#CheckBoardModule',
  //             data: {
  //                 permission: 'Pages.CheckBoard',
  //                 preload: true
  //             }
  //         }]
  //     }]
  // },
  {
    path: "home",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "./../home/home.module#HomeModule",
            data: {
              permission: "Home",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "user",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/user/user.module#UserSModule",
            data: {
              permission: "Admin.Users",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "day-off",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/day-off/day-off.module#DayOffModule",
            data: {
              permission: "DayOff",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "off-day",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/off-day/off-day.module#OffDayModule",
            data: {
              permission: "DayOff",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "manage-absence-types",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/manage-absence-types/manage-absence-types.module#ManageAbsenceTypesModule",
            data: {
              permission: "Admin.LeaveTypes",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "branchs",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/branchs/branchs.module#BranchsModule",
            data: {
              permission: "Admin.Branchs",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    //TODO Permission
    path: "position",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/position/position.module#PositionModule",
            data: {
              permission: "Admin.Position",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "manage-working-times",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "./manage-working-times/manage-working-times.module#ManageWorkingTimesModule",
            data: {
              permission: "ManageWorkingTime",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "overtime-setting",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "./overtime-setting/overtime-setting.module#OvertimeSettingModule",
            data: {
              permission: "OverTimeSetting",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "auditlog",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "./auditlog/auditlog.module#AuditlogModule",
            data: {
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  // {
  //     path: 'user-salary',
  //     component: MainComponent,
  //     canActivate: [AppRouteGuard],
  //     children: [{
  //         path: '',
  //         children: [{
  //             path: '',
  //             loadChildren: '../modules/user-salary/user-salary.module#UserSalaryModule',
  //             data: {
  //                 permission: 'User.Salary',
  //                 preload: true
  //             }
  //         }]
  //     }]
  // },
  // {
  //     path: 'user-salary-month',
  //     component: MainComponent,
  //     canActivate: [AppRouteGuard],
  //     children: [{
  //         path: '',
  //         children: [
  //             {
  //                 path: '',
  //                 loadChildren: '../modules/user-salary-month/user-salary-month.module#UserSalaryMonthModule',
  //                 data: {
  //                     permission: 'User.SalaryMonth',
  //                     preload: true
  //                 },
  //             }
  //         ]
  //     }]
  // },
  {
    path: "tardiness-leave-early",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/report/tardiness-leave-early/tardiness-leave-early.module#TardinessLeaveEarlyModule",
            data: {
              permission: "Report.TardinessLeaveEarly",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "komu-tracker",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/report/komu-tracker/komu-tracker.module#KomuTrackerModule",
            data: {
              permission: "Report.KomuTracker",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "tardiness-leave-early-detail",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/report/tardiness-leave-early/tardiness-detail/tardiness-detail.module#TardinessDetailModule",
            data: {
              permission: "",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "timesheets",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/timesheet/timesheet.module#TimesheetModule",
            data: {
              permission: "Timesheet",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "review",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/review/review.module#ReviewModule",
            data: {
              permission: "ReviewsTTS",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "review-detail",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/review/review-detail/review-detail.module#ReviewDetailModule",
            data: {
              permission: "ReviewDetail",
              preload: true,
            },
            // canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "over-time",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/report/over-time/over-time.module#OverTimeModule",
            data: {
              permission: "Report.OverTime",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    path: "interns-info",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/report/interns-info/interns-info.module#InternsInfoModule",
            data: {
              permission: "Report.InternsInfo",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "normal-working",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/report/normal-working/normal-working.module#NormalWorkingModule",
            data: {
              permission: "Report.NormalWorking",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "leave-day-of-user/:id",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/off-day-project/leave-day-of-user/leave-day-of-user.module#LeaveDayOfUserModule",
            data: {
              permission: "AbsenceDayByProject.ViewDetail",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "view-leave-day-of-user/:id",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/off-day-project-for-user/view-leave-day-of-user/view-leave-day-of-user.module#ViewLeaveDayOfUserModule",
            data: {
              permission: "AbsenceDayOfTeam.ViewDetail",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "absence-day",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/absence-day/absence-day.module#AbsenceDayModule",
            data: {
              permission: "MyAbsenceDay",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "absence-request",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/approve-absence-request/approve-absence-request.module#ApproveAbsenceRequestModule",
            data: {
              permission: "AbsenceDay",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "off-day-project",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/off-day-project/off-day-project.module#OffDayProjectModule",
            data: {
              permission: "AbsenceDayByProject",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    path: "off-day-project-for-user",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/off-day-project-for-user/off-day-project-for-user.module#OffDayProjectForUserModule",
            data: {
              permission: "AbsenceDayOfTeam",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },

  {
    path: "my-working-time",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/my-working-time/my-working-time.module#MyWorkingTimeModule",
            data: {
              permission: "MyWorkingTime",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
  {
    //TODO Permission
    path: "retro",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren: "../modules/retros/retro/retro.module#RetroModule",
            data: {
              permission: "Retro.ManageRetro",
              preload: true,
            },
          },
        ],
      },
    ],
  },
  {
    //TODO Permission
    path: "retro-detail",
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [
      {
        path: "",
        children: [
          {
            path: "",
            loadChildren:
              "../modules/retros/retro/retro-detail/retro-detail.module#RetroDetailModule",
            data: {
              permission: "Retro.ManageRetro.RetroDetail",
              preload: true,
            },
            canActivate: [AppRouteGuard],
          },
        ],
      },
    ],
  },
    {
        path: 'my-profile',
        component: MainComponent,
        canActivate: [AppRouteGuard],
        children: [{
            path: '',
            children: [
                {
                    path: '',
                    loadChildren: '../modules/my-profile/my-profile.module#MyProfileModule',
                    data: {
                        permission: '',
                        preload: true
                    },
                    canActivate: [AppRouteGuard],
                }
            ]
        }]
  },
  {
    path: 'capabilities',
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [{
        path: '',
        children: [{
            path: '',
            loadChildren: '../modules/capabilities/capability.module#CapabilityModule',
            data: {
                permission: 'Admin.Capability',
                preload: true
            }
        }]
    }]
},
{
    path: 'capability-settings',
    component: MainComponent,
    canActivate: [AppRouteGuard],
    children: [{
        path: '',
        children: [{
            path: '',
            loadChildren: '../modules/capability-settings/capability-setting.module#CapabilitySettingModule',
            data: {
                permission: 'Admin.CapabilitySetting',
                preload: true
            }
        }]
    }]
},
{
  path: 'background-jobs',
  component: MainComponent,
  canActivate: [AppRouteGuard],
  children: [{
    path: '',
    loadChildren: '../modules/background-jobs/background-job.module#BackgroundJobModule',
    data: {
      permission: '',
      preload: true
  }
  }]
},
{
  path: "team-building-hr",
  component: MainComponent,
  canActivate: [AppRouteGuard],
  children: [
    {
      path: "",
      children: [
        {
          path: "",
          loadChildren:
            "../modules/team-building/team-building-hr/team-building-hr.module#TeamBuildingHRModule",
          data: {
            permission: "TeamBuilding.DetailHR",
            preload: true,
          },
          canActivate: [AppRouteGuard],
        },
      ],
    },
  ],
},
{
  path: "team-building-pm",
  component: MainComponent,
  canActivate: [AppRouteGuard],
  children: [
    {
      path: "",
      children: [
        {
          path: "",
          loadChildren:
            "../modules/team-building/team-building-pm/team-building-pm.module#TeamBuildingPMModule",
          data: {
            permission: "TeamBuilding.DetailPM",
            preload: true,
          },
          canActivate: [AppRouteGuard],
        },
      ],
    },
  ],
},
{
  path: "team-building-project",
  component: MainComponent,
  canActivate: [AppRouteGuard],
  children: [
    {
      path: "",
      children: [
        {
          path: "",
          loadChildren:
            "../modules/team-building/team-building-project/team-building-project.module#TeamBuildingProjectModule",
          data: {
            permission: "TeamBuilding.Project",
            preload: true,
          },
          canActivate: [AppRouteGuard],
        },
      ],
    },
  ],
},
{
  path: "team-building-request",
  component: MainComponent,
  canActivate: [AppRouteGuard],
  children: [
    {
      path: "",
      children: [
        {
          path: "",
          loadChildren:
            "../modules/team-building/team-building-request/team-building-request.module#TeamBuildingRequestModule",
          data: {
            permission: "TeamBuilding.Request",
            preload: true,
          },
          canActivate: [AppRouteGuard],
        },
      ],
    },
  ],
},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class MainRoutingModule {}
