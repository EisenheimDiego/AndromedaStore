"use client";
import React, { useState, useEffect } from 'react';
import 'bootstrap/dist/css/bootstrap.css';
import '../ui/global.css';
import Link from 'next/link'


export default function CartPage() {
    const [cart, setCar] = useState({});
    const [subtotal, setSubtotal] = useState(0);
    const taxes = 0.13;

    useEffect(() => {
        const itemsCart = JSON.parse(localStorage.getItem('cartItems') || '[]');
        setCar(itemsCart);
    }, []);


    useEffect(() => {
        let calculateSubtotal = 0;
        Object.values(cart).forEach(item => {
            calculateSubtotal += item.price;
        });
        setSubtotal(calculateSubtotal);
    }, [cart]);


    const cartBooks = () => {
        return (
            <div className='container'>
                <div className="row my-3">
                {Object.values(cart).map((item) => (
                    <div key={item.id} className="col-sm-3 mb-4" >
                        <div className="card">
                            <img src={item.imgUrl} className="card-img-top" alt={item.name} />
                            <div className="card-body">
                                <div className="text-center">
                                    <h4 className="card-title my-3">{item.name}</h4>
                                    <p className="card-text my-3">{item.description}</p>
                                    <p className="card-text my-3">Price: ₡{item.price}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                ))}
                </div>
            </div>
        )
    }

    return (

        <div style={{ backgroundColor: 'silver' }}>

            <header className="p-3 text-bg-dark">
                <div className="row" style={{ color: 'gray' }}>
                    <div className="col-sm-9">
                        <Link href="/">
                            <img src="Logo1.jpg" style={{ height: '75px', width: '200px', margin: '1.4rem' }} className="img-fluid" />
                        </Link>
                    </div>
                    <div className="col-sm-3 d-flex justify-content-end align-items-center">
                        <Link href="/">
                            <button className="btn btn-dark"> Go Home</button>
                        </Link>
                    </div>
                </div>
            </header>

            <div className='container'>
                <h2 className='text-left mt-5 mb-5'>Shopping cart</h2>
                {cartBooks()}
            </div>

            <div className="d-flex justify-content-end" style={{ position: 'fixed', bottom: '65px', right: '10px', zIndex: '1000' }}>
                <div style={{ backgroundColor: 'white', margin: '100' }}>
                    <h2>Subtotal: ₡{subtotal}</h2>
                    <h2 className='my-3'>Tax: 13%</h2>
                    <h2 className='my-3'>Total: ₡{(subtotal + (subtotal * taxes)).toFixed(2)} </h2>
                    {cart.length > 0 ? (
                        <Link href="/payment">
                            <button className="btn btn-success" style={{ display: 'flex', justifyContent: 'center' }} onClick={() => console.log('Buying successfully')}>Click to Buy</button>
                        </Link>
                    ) : (
                        <button className="btn btn-success" style={{ display: 'flex', justifyContent: 'center' }} disabled>Click to Buy</button>
                    )}
                </div>
            </div>
            <footer className='footer'>
                <div className="text-center p-3">
                    <h5 className="text-light">Dev: Paula Chaves</h5>
                </div>
            </footer>
        </div>
    );
};