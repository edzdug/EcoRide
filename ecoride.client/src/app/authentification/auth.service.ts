import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';

interface LoginResponse {
  token: string;
  user: any;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _userSubject = new BehaviorSubject<any | null>(null);
  user$: Observable<any | null> = this._userSubject.asObservable();

  constructor(private http: HttpClient) {
    // Au démarrage, on peut vérifier s'il y a un token en localStorage
    const userJson = localStorage.getItem('user');
    if (userJson) {
      this._userSubject.next(JSON.parse(userJson));
    }
  }

  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/Authentification/login', { email, password }).pipe(
      tap(response => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        this._userSubject.next(response.user);
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this._userSubject.next(null);
  }

  get isLoggedIn() {
    return !!this._userSubject.value;
  }
}
