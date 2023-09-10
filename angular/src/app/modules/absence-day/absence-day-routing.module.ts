import {NgModule} from '@angular/core';
import {Routes, RouterModule} from '@angular/router';
import {AbsenceDayComponent} from '@app/modules/absence-day/absence-day.component';

const routes: Routes = [{
    path: '', component: AbsenceDayComponent
}];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class AbsenceDayRoutingModule {
}
