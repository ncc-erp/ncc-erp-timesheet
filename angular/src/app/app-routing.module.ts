import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AppComponent } from './app.component';
import { AppRouteGuard } from '@shared/auth/auth-route-guard';
import { HomeComponent } from './home/home.component';
import { UsersComponent } from './users/users.component';
import { TenantsComponent } from './tenants/tenants.component';
import { RolesComponent } from 'app/roles/roles.component';
import { ChangePasswordComponent } from './users/change-password/change-password.component';
import { ConfigurationComponent } from './configuration/configuration.component';
import { NoPermissionComponent } from '@shared/interceptor-errors/no-permission/no-permission.component';
import { EditRoleComponent } from './roles/edit-role/edit-role.component';

const routes: Routes = [
    {
        path: '',
        component: AppComponent,
        // children: [
        //     { path: 'home', component: HomeComponent, data: { permission: 'Pages.Report' }, canActivate: [AppRouteGuard] },
        //     { path: 'users', component: UsersComponent, data: { permission: 'Pages.Users' }, canActivate: [AppRouteGuard] },
        //     { path: 'roles', component: RolesComponent, data: { permission: 'Pages.Roles' }, canActivate: [AppRouteGuard] },
        //     { path: 'tenants', component: TenantsComponent, data: { permission: 'Pages.Tenants' }, canActivate: [AppRouteGuard] },
        //     { path: 'update-password', component: ChangePasswordComponent },
        //     { path: 'configuration', component: ConfigurationComponent, data: { permission: 'Pages.EmailConfiguration' }, canActivate: [AppRouteGuard] },
        //     { path: 'no-permission', component: NoPermissionComponent },
        //     { path: 'roles/edit/:id', component: EditRoleComponent, data: { permission: 'Pages.Roles' }, canActivate: [AppRouteGuard] },
        //     { path: 'roles/create', component: CreateRoleComponent, data: { permission: 'Pages.Roles' }, canActivate: [AppRouteGuard] },
        // ]
        children: [
            { path: 'users', component: UsersComponent, data: { permission: 'Admin.Users' }, canActivate: [AppRouteGuard] },
            { path: 'roles', component: RolesComponent, data: { permission: 'Admin.Roles' }, canActivate: [AppRouteGuard] },
            { path: 'tenants', component: TenantsComponent, data: { permission: 'Tenants' }, canActivate: [AppRouteGuard] },
            { path: 'update-password', component: ChangePasswordComponent },
            { path: 'configuration', component: ConfigurationComponent, data: { permission: 'Admin.Configuration' }, canActivate: [AppRouteGuard] },
            { path: 'no-permission', component: NoPermissionComponent },
            { path: 'roles/edit/:id', component: EditRoleComponent, data: { permission: 'Admin.Roles.Edit' }, canActivate: [AppRouteGuard] },
            { path: 'roles/view-detail/:id', component: EditRoleComponent, data: { permission: 'Admin.Roles.ViewDetail' }, canActivate: [AppRouteGuard] },            
        ]
    },
    {
        path: 'main',
        component: AppComponent,
        canActivate: [AppRouteGuard],
        children: [{
            path: '',
            children: [
                {
                    path: '',
                    loadChildren: './modules/main.module#MainModule',
                    data: {
                        preload: true
                    }
                }
            ]
        }]
    }
]

@NgModule({
    imports: [
        RouterModule.forChild(routes)
    ],
    exports: [RouterModule]
})
export class AppRoutingModule { }
