import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { UserWhitelistComponent } from './user-whitelist.component';

const routes: Routes = [
    {
        path: '',
        component: UserWhitelistComponent
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UserWhitelistRoutingModule { }