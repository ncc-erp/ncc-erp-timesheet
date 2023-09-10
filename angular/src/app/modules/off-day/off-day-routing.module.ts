import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { OffDayComponent } from './off-day.component';

const routes: Routes = [
    {path: '', component: OffDayComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class OffDayRoutingModule {}