import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DayOffComponent } from './day-off.component';

const routes: Routes = [
    {path: '', component: DayOffComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class DayOffRoutingModule {}