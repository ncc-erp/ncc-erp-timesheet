import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { TardinessDetailComponent } from './tardiness-detail.component';

describe('TardinessDetailComponent', () => {
  let component: TardinessDetailComponent;
  let fixture: ComponentFixture<TardinessDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TardinessDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TardinessDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
