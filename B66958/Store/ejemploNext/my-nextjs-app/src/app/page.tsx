'use client';
import "bootstrap/dist/css/bootstrap.min.css";
import { useState, useEffect, Suspense } from 'react';
import NavBar from "./navbar/page";
import Cart from './Cart/page';
import Alert from 'react-bootstrap/Alert';
import Carousel from 'react-bootstrap/Carousel';
import { Dropdown } from "react-bootstrap";
import { useSearchParams, useRouter, usePathname } from 'next/navigation';
import DOMPurify from 'dompurify';
import { CartState, PaymentMethod } from "./types/Cart";
import './css/messages.css';
import WebSocketMessage from "./navbar/WebSocketMessage";

export default function Home() {

  const searchParams = useSearchParams()
  const router = useRouter()
  const pathname = usePathname()
  const querySearch = searchParams.get('query')
  const categoriesSearch = searchParams.getAll('categories')

  const [isErrorShowing, setIsErrorShowing] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const [products, setProducts] = useState([]);
  const [categories, setCategories] = useState([]);
  const [selectedCategories, setSelectedCategories] = useState([]);
  const [productQuery, setProductQuery] = useState('');

  const [isCartActive, setIsCartActive] = useState(false);

  const [messages, setMessages] = useState<WebSocketMessage[]>([]);
  const [isMessageListVisible, setMessageListVisible] = useState<boolean>(false);

  const [count, setCount] = useState(0);
  const [idList, setIdList] = useState([]);

  const [cartLoaded, setCartLoaded] = useState(false);
  const [cart, setCart] = useState<CartState>({
    carrito: {
      productos: [],
      subtotal: 0,
      porcentajeImpuesto: 0,
      total: 0,
      direccionEntrega: '',
      metodoDePago: -1
    },
    metodosDePago: [],
    necesitaVerificacion: false
  });

  const toggleMessageList = () => {
    setMessageListVisible(!isMessageListVisible);
  };
  
  let environmentUrl = process.env.NEXT_PUBLIC_NODE_ENV;

  const cleanHTML = (html : string) => {
    return DOMPurify.sanitize(html, {
      ADD_TAGS: ['iframe'],
      ADD_ATTR: ['allow', 'allowfullscreen', 'frameborder', 'scrolling']
    });
  };

  const categoriesSearchString = Array.isArray(categoriesSearch) ? categoriesSearch.join(',') : categoriesSearch;
  useEffect(() => {
    if (categoriesSearch.length !== 0 || querySearch !== null) {
      let urlToFilterCategories = buildQueryString(categoriesSearch);
      let searchedQuerie = querySearch ? `query=${querySearch}` : '';
      let urlToFilterQuery = urlToFilterCategories ? (searchedQuerie ? `&${searchedQuerie}` : '') : (searchedQuerie ? `?${searchedQuerie}` : '')
      if (categoriesSearch)
        setSelectedCategories(categoriesSearch);
      const fetchUrl = `${environmentUrl}/api/store/products` + urlToFilterCategories + urlToFilterQuery;
      const fetchData = async () => {
        const res = await fetch(fetchUrl, {
          method: 'GET',
          headers: {
            'content-type': 'application/json'
          }
        });
        var productsForQuerySearch = await res.json();
        setProducts(productsForQuerySearch);
      };
      fetchData();
    } else {
      const fetchStore = async () => {
        try {
          const result = await getData();
          setProducts(result.productsInStore);
        } catch (error) {
          setErrorMessage(String(error))
          setIsErrorShowing(true)
        }
      };
      fetchStore();
    }
  }, [querySearch, categoriesSearchString]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        const result = await getData();
        const paymentTypes : PaymentMethod[] = result.paymentMethods;
        setProducts(result.productsInStore);
        setCategories(result.categoriesInStore);
        setCart(cart => ({
          ...cart,
          carrito: {
            ...cart.carrito,
            porcentajeImpuesto: result.taxPercentage
          },
          metodosDePago: paymentTypes
        }));
      } catch (error) {
        setErrorMessage(error)
        setIsErrorShowing(true)
      }
    };
    fetchData();
  }, []);

  useEffect(() => {
    const storedCart = localStorage.getItem('cart');
    let cartExistsAndIsLoaded = storedCart && !cartLoaded;
    if (cartExistsAndIsLoaded) {
      setCart(JSON.parse(storedCart));
      setCartLoaded(true);
    }
  }, [cartLoaded]);

  useEffect(() => {
    if (cartLoaded) {
      localStorage.setItem('cart', JSON.stringify(cart));
    }
    setCount(cart?.carrito?.productos?.length ?? 0);
  }, [cart, cartLoaded]);

  const clearProducts = () => {
    localStorage.removeItem('cart');
    setIdList([]);
    const updatedCart: CartState = {
      ...cart,
      carrito: {
        ...cart.carrito,
        productos: [],
        subtotal: 0,
        total: 0,
        direccionEntrega: '',
        metodoDePago: -1
      },
      necesitaVerificacion: false
    }
    setCart(updatedCart);
  }

  function productAlreadyAdded({ product }) {
    return idList.includes(product.uuid);
  }

  function addProductToCart({ product }: any) {
    const newProduct = {
      ...product,
      quantity: 1,
    };

    const newProductos = [...(cart.carrito.productos || []), newProduct];

    setCart(cart => ({
      ...cart,
      carrito: {
        ...cart.carrito,
        productos: newProductos
      }
    }));
    setCartLoaded(true);
  }

  function calculateTotals({ product }: any) {
    const newSubTotal = cart.carrito.subtotal + product.price;
    const newTotal = newSubTotal + (newSubTotal * (cart.carrito.porcentajeImpuesto / 100));

    setCart(cart => ({
      ...cart,
      carrito: {
        ...cart.carrito,
        subtotal: newSubTotal,
        total: newTotal
      }
    }));
  }

  const handleAddToCart = ({ product }: any) => {
    if (!productAlreadyAdded({ product })) {
      idList.push(product.uuid);
      addProductToCart({ product });
      calculateTotals({ product });
      setCount(count + 1);
    }
  };

  const toggleCart = ({ action }: any) => {
    setIsCartActive(action ? true : false);
  };

  function updateBrowserUrl(categoriesSelected: Array<Number>) {
    let overOneCategorySelected = categoriesSelected.length > 0
    let urlToBeDisplayed = ''
    let categoriesParams = overOneCategorySelected ? buildQueryString(categoriesSelected) : '?'
    let queryParams = productQuery ? (overOneCategorySelected ? `&query=${productQuery}` : `query=${productQuery}`) : ''
    urlToBeDisplayed = categoriesParams + queryParams

    return urlToBeDisplayed
  }

  async function getData() {
    try {
      const res = await fetch(`${environmentUrl}/api/store`);
      if (!res.ok) {
        throw new Error('Failed to fetch data');
      }
      return res.json();
    } catch (error) {
      throw error;
    }
  }

  function buildQueryString(categories: any) {
    if (!categories || categories.length === 0) {
      return '';
    }
    const queryString = categories.map(category => `categories=${category}`).join('&');
    return `?${queryString}`;
  }

  const searchProduct = () => {
    if (!productQuery) {
      setErrorMessage('Por favor ingrese una consulta');
      setIsErrorShowing(true);
    } else {
      router.push(pathname + updateBrowserUrl(selectedCategories))
    }
  }

  const Product = ({ product, handleAddToCart }) => {
    const { uuid, name, description, imageUrl, price } = product;
    const sanitizedDescription = cleanHTML(description);
    return (
      <div className="card" style={{ width: '20rem' }}>
        <div className="col">
          <div className="card-body">
            <img className="card-img-top"
              src={imageUrl}
              width="500" height="110" />
            <h5>{name}</h5>
            <p>Precio: ${price}</p>
            <p>Descripción: <span dangerouslySetInnerHTML={{ __html: sanitizedDescription }} /></p>
            <button type="button" className="btn btn-light" onClick={() => handleAddToCart({ product })}>Comprar</button>
          </div>
        </div>
      </div>
    );
  };

  const MyRow = () => {

    function handleCategoryChange(newCategory: string) {
      const updatedCategories = selectedCategories.includes(newCategory)
        ? selectedCategories.filter(category => category !== newCategory)
        : [...selectedCategories, newCategory];

      setSelectedCategories(updatedCategories);
      router.push(pathname + updateBrowserUrl(updatedCategories))
    }

    return (
      <>
        <div className="row">
          <div className="col-auto">
            <h1>Lista de productos</h1>
            <Dropdown>
              <Dropdown.Toggle variant="success" id="dropdown-basic">
                Todas las categorías
              </Dropdown.Toggle>
              <Dropdown.Menu>
                {categories.map(category => (
                  <Dropdown.Item
                    key={category.id}
                    onClick={() => handleCategoryChange(String(category.id))}
                    active={
                      selectedCategories.includes(category.id) || selectedCategories.includes(String(category.id))}>
                    {category.name}
                  </Dropdown.Item>
                ))}
              </Dropdown.Menu>
            </Dropdown>
          </div>
        </div>
        <div className="row justify-content-md-center">
          {products.map(product => <Product key={product.uuid} product={product} handleAddToCart={handleAddToCart} />)}
        </div>
      </>
    );
  };

  const CarouselBootstrap = () => {
    return (
      <Carousel>
        {products.map(product =>
          <Carousel.Item key={product.uuid}>
            <img
              className="d-block w-100"
              src={product.imageUrl}
              alt="First slide"
            />
            <Carousel.Caption>
              <h3>{product.name}</h3>
              <p>${product.price}</p>
              <button type="button" className="btn btn-light" onClick={() => handleAddToCart({ product })}>Comprar</button>
            </Carousel.Caption>
          </Carousel.Item>)}
      </Carousel>
    );
  }

  return ( <Suspense fallback={<div>Loading...</div>}>
    <div className="d-grid gap-2">
      <NavBar productCount={count} toggleCart={(action) => toggleCart({ action })}
        searchFunction={searchProduct} setQuery={setProductQuery}
        setMessages={setMessages} toggleMessageList={toggleMessageList} />
        {isMessageListVisible && (
                <div className="message-list">
                    {messages.map(message => (
                        <div key={message.Id} className="message">
                          <span dangerouslySetInnerHTML={{ __html: cleanHTML(message.Text) }} />
                        </div>
                    ))}
                </div>
            )}
      {isCartActive ? <Cart cart={cart} setCart={setCart}
        toggleCart={(action) => toggleCart({ action })} clearProducts={clearProducts} /> : <><MyRow /> <CarouselBootstrap /></>}
      {isErrorShowing ?
        <div
          style={{
            position: 'fixed',
            bottom: 20,
            right: 20,
            zIndex: 9999,
          }}
        >
          <Alert variant="danger" onClose={() => setIsErrorShowing(false)} dismissible>
            <Alert.Heading>Error</Alert.Heading>
            <p>{errorMessage.toString()}</p>
          </Alert>
        </div> : ''
      }
    </div>
    </Suspense>
  );
}
