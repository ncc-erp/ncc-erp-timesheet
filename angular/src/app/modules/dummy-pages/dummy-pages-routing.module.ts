import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AppRouteGuard } from '@shared/auth/auth-route-guard';
import { ComplainMailComponent } from './complain-mail/complain-mail.component';
import { ConfirmMailComponent } from './confirm-mail/confirm-mail.component';
import { DummyPagesComponent } from './dummy-pages.component';

const routes: Routes = [{
  path : 'public',
  component: DummyPagesComponent,
  children: [
    {
        path: "complain-mail",
        component: ComplainMailComponent,
        canActivate: [AppRouteGuard]
    },
    {
        path: "confirm-mail",
        component: ConfirmMailComponent,
        canActivate: [AppRouteGuard]
    }
  ]
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DummyPageRoutingModule { }
