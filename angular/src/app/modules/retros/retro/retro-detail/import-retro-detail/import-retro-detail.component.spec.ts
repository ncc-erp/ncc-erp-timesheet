import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ImportRetroDetailComponent } from './import-retro-detail.component';

describe('ImportRetroDetailComponent', () => {
  let component: ImportRetroDetailComponent;
  let fixture: ComponentFixture<ImportRetroDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImportRetroDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImportRetroDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
