//MARIANO DURAN ARTAVIA C12599
'use client';
import React, { useState, useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.css';
import '../ui/globals.css';
import Link from 'next/link';

const EjemploPage: React.FC = () => {
  const [carrito, setCarrito] = useState({
    productos: [], 
    subtotal: 0,
    total: 0,
    direccionEntrega: '',
    metodosDePago: {},
    comprobante: '',
    confirmacion: '',
    numeroCompra: 0,
    numeroPago: 0,
  });

  useEffect(() => {
    const cartData = JSON.parse(localStorage.getItem('cartData') || '{}');
    setCarrito(cartData);
  }, []);

  useEffect(() => {
    let hayProductosEnCarrito = carrito.productos && carrito.productos.length > 0;
    if (hayProductosEnCarrito) {
        let subtotalCalculado = carrito.productos.reduce((total, item) => total + item.price, 0);
        let totalCalculado = (subtotalCalculado * 0.13) + subtotalCalculado;
        setCarrito(prevCarrito => ({
            ...prevCarrito,
            subtotal: subtotalCalculado,
            total: totalCalculado
        }));
    }
  }, [carrito.productos]);

  const handleClearCart = () => {
    setCarrito({
      productos: [],
      subtotal: 0,
      total: 0,
      direccionEntrega: '',
      metodosDePago: {},
      comprobante: '',
      confirmacion: '',
      numeroCompra: 0,
      numeroPago: 0,
    });
    localStorage.removeItem('cartData');
  };

  return (
    <div>
      <h1>Carrito de Compras</h1>
      <div className="row my-3">
        {carrito.productos && carrito.productos.length > 0 && carrito.productos.map(item => (
          <div key={item.id} className="col-sm-3 mb-4">
            <div className="card">
              <img src={item.imageUrl} className="card-img-top" alt={item.name} />
              <div className="card-body">
                <div className="text-center">
                  <h5 className="card-title my-3">{item.name}</h5>
                  <p className="card-text my-3">{item.description}</p>
                  <p className="card-text my-3">Precio: ${item.price}</p>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>
      <div className="text-right">
        <h2>Subtotal: ${carrito.subtotal ? carrito.subtotal.toFixed(2) : '0.00'}</h2>
        <h2 className='my-3'>Total: ${carrito.total ? carrito.total.toFixed(2) : '0.00'}</h2>
      </div>
      <div className="text-right">
        {carrito.productos && carrito.productos.length > 0 && (
          <>
            <button className="btn btn-danger mr-3" onClick={handleClearCart}>Vaciar Carrito</button>
            <Link href="/pay">
              <button className="btn btn-primary">Comprar</button>
            </Link>
          </>
        )}
      </div>
    </div>
  );
}

export default EjemploPage;
