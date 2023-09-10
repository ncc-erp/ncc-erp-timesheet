import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UserSecondComponent } from './user.component';

const routes: Routes = [
    {path: '', component: UserSecondComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class UserRoutingModule {}