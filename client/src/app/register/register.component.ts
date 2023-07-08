import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancleRegister = new EventEmitter();
  model: any = {};

  constructor(private accountService: AccountService) { }

  ngOnInit(): void {
  }

  register(){
    return this.accountService.register(this.model).subscribe({
      next: () => {
        this.cancle();
      },
      error: error => console.log(error)
    })
  }
  
  cancle() {
    this.cancleRegister.emit(false)
  }


}
