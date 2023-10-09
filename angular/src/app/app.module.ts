import { UploadAvatarComponent } from './modules/user/upload-avatar/upload-avatar.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { JsonpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';

import { ModalModule } from 'ngx-bootstrap';
import { NgxPaginationModule } from 'ngx-pagination';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import { AbpModule } from '@abp/abp.module';

import { ServiceProxyModule } from '@shared/service-proxies/service-proxy.module';
import { SharedModule } from '@shared/shared.module';

import {MatFormFieldModule} from '@angular/material/form-field';

import { HomeComponent } from '@app/home/home.component';
import { TopBarComponent } from '@app/layout/topbar.component';
import { TopBarLanguageSwitchComponent } from '@app/layout/topbar-languageswitch.component';
import { SideBarUserAreaComponent } from '@app/layout/sidebar-user-area.component';
import { SideBarNavComponent } from '@app/layout/sidebar-nav.component';
import { SideBarFooterComponent } from '@app/layout/sidebar-footer.component';
import { RightSideBarComponent } from '@app/layout/right-sidebar.component';
// tenants
import { TenantsComponent } from '@app/tenants/tenants.component';
import { CreateTenantDialogComponent } from './tenants/create-tenant/create-tenant-dialog.component';
import { EditTenantDialogComponent } from './tenants/edit-tenant/edit-tenant-dialog.component';
// roles
import { RolesComponent } from '@app/roles/roles.component';
// users
import { UsersComponent } from '@app/users/users.component';
import { CreateEditUserComponent } from './users/create-edit-user/create-edit-user-dialog.component';
import { ChangePasswordComponent } from './users/change-password/change-password.component';
import { ResetPasswordDialogComponent } from './users/reset-password/reset-password.component';

import { PopupsComponent } from './home/popups/popups.component';

//configuration
import { ConfigurationComponent } from './configuration/configuration.component';
import { MyTimeSheetsModule } from './modules/mytimesheet/mytimesheets.module';
import { ImageCropperModule } from 'ngx-image-cropper';
import {MatListModule} from '@angular/material/list';


// google single sign-on
import { SocialLoginModule, AuthServiceConfig } from 'angularx-social-login';
import { GoogleLoginProvider } from 'angularx-social-login';
import { AppConsts } from '@shared/AppConsts';
import { NgxCurrencyModule } from 'ngx-currency';
import { EditSidebarComponent } from './layout/edit-sidebar/edit-sidebar.component';
import { EditRoleComponent } from './roles/edit-role/edit-role.component';
import { CreateRoleDialogComponent } from './roles/create-role/create-role-dialog.component';
import { UpdatePunishMoneyComponent } from './configuration/update-punish-money/update-punish-money.component';
import { environment } from '../environments/environment';
import * as Sentry from "@sentry/browser";

let config = new AuthServiceConfig([
  {
    id: GoogleLoginProvider.PROVIDER_ID,
    provider: new GoogleLoginProvider(AppConsts.googleClientAppId != '' ? AppConsts.googleClientAppId : 'googleClientAppId' )
  }
]);

export function provideConfig() {
  return config;
}

Sentry.init({
  dsn: environment.sentryDsn,
})

@NgModule({
  declarations: [
    AppComponent,
    TopBarComponent,
    TopBarLanguageSwitchComponent,
    SideBarUserAreaComponent,
    SideBarNavComponent,
    SideBarFooterComponent,
    RightSideBarComponent,
    // tenants
    TenantsComponent,
    CreateTenantDialogComponent,
    EditTenantDialogComponent,
    // roles
    RolesComponent,
    EditRoleComponent,
    CreateRoleDialogComponent,
    // users
    UsersComponent,
    CreateEditUserComponent,
    ChangePasswordComponent,
    ResetPasswordDialogComponent,
    PopupsComponent,
    //configurations
    ConfigurationComponent,
    UploadAvatarComponent,
    EditSidebarComponent,
    UpdatePunishMoneyComponent,

  ],
  imports: [
    SocialLoginModule,
    ImageCropperModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    JsonpModule,
    ModalModule.forRoot(),
    AbpModule,
    AppRoutingModule,
    ServiceProxyModule,
    SharedModule,
    NgxPaginationModule,
    MyTimeSheetsModule,
    MatFormFieldModule,
    NgxCurrencyModule,
    MatListModule,
  ],
  providers: [
    {
      provide: AuthServiceConfig,
      useFactory: provideConfig
    }
  ],
  entryComponents: [
    EditSidebarComponent,
    // tenants
    CreateTenantDialogComponent,
    EditTenantDialogComponent,
    // roles
    CreateRoleDialogComponent,
    // users
    CreateEditUserComponent,
    ResetPasswordDialogComponent,
    UploadAvatarComponent,
    UpdatePunishMoneyComponent
  ]
})
export class AppModule {}
