import { Injectable } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { HttpInterceptor, HttpRequest, HttpHandler } from '@angular/common/http';
 
export const jwtInterceptor : HttpInterceptorFn = (req,next) =>{
    const token = localStorage.getItem('authToken');
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next(req);
  
}
