import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, map } from 'rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  baseUrl = 'https://localhost:5001/api/'
  private currentUserSource = new BehaviorSubject<User | null>(null); // BehaviorSubject lưu trữ giá trị hiện tại và phát ra giá trị mới cho tất cả người nghe(subscribers)
  currentUser$ = this.currentUserSource.asObservable();

  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + "account/login", model).pipe(
      map((respone: User) => {
        const user = respone
        console.log("user", respone)
        if(user) {
          localStorage.setItem('user', JSON.stringify(user))
          this.currentUserSource.next(user);
        }
      })
    )
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map(user => {
        if(user) {
          localStorage.setItem('user', JSON.stringify(user));
          this.currentUserSource.next(user)
        }
      })
    )
  }
  setCurrentUser(user: User) {
    this.currentUserSource.next(user)
  }

  logOut() {
    localStorage.removeItem('user');
    this.currentUserSource.next(null);
  }
}
