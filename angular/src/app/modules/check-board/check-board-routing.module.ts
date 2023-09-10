import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CheckBoardComponent } from './check-board.component';

const routes: Routes = [
    {path: '', component: CheckBoardComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class CheckBoardRoutingModule {}