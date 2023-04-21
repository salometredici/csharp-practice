--
-- PostgreSQL database dump
--

-- Dumped from database version 15.2
-- Dumped by pg_dump version 15.2

-- Started on 2023-04-21 05:48:24

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 3423 (class 1262 OID 16398)
-- Name: testdb; Type: DATABASE; Schema: -; Owner: postgres
--

CREATE DATABASE testdb WITH TEMPLATE = template0 ENCODING = 'UTF8' LOCALE_PROVIDER = libc LOCALE = 'English_United States.1252';


ALTER DATABASE testdb OWNER TO postgres;

\connect testdb

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 7 (class 2615 OID 16400)
-- Name: transactions; Type: SCHEMA; Schema: -; Owner: test_user
--

CREATE SCHEMA transactions;


ALTER SCHEMA transactions OWNER TO test_user;

--
-- TOC entry 2 (class 3079 OID 16508)
-- Name: pldbgapi; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS pldbgapi WITH SCHEMA public;


--
-- TOC entry 3426 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION pldbgapi; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION pldbgapi IS 'server-side support for debugging PL/pgSQL functions';


--
-- TOC entry 265 (class 1255 OID 16572)
-- Name: func_get_account_by_id(integer); Type: FUNCTION; Schema: transactions; Owner: postgres
--

CREATE FUNCTION transactions.func_get_account_by_id(p_acc_id integer) RETURNS TABLE(accountid integer, currencyid integer, currencycode character varying, currency character varying, balance real, userid integer, userfullname character varying)
    LANGUAGE plpgsql
    AS $$
BEGIN
	RETURN query
		SELECT 
			a.id as AccountId,
			c.id as CurrencyId,
			a.currency_code as CurrencyCode,
			c.description as Currency,
			a.balance,
			a.user_id as UserId,
			a.user_full_name as UserFullName	
		FROM
			transactions.accounts a
		INNER JOIN transactions.currencies c
			ON c.id = a.currency_id
		WHERE a.id = p_acc_id;
END;
$$;


ALTER FUNCTION transactions.func_get_account_by_id(p_acc_id integer) OWNER TO postgres;

--
-- TOC entry 230 (class 1255 OID 16462)
-- Name: func_get_commission_rate(); Type: FUNCTION; Schema: transactions; Owner: postgres
--

CREATE FUNCTION transactions.func_get_commission_rate() RETURNS real
    LANGUAGE plpgsql
    AS $$
BEGIN
	RETURN 0.01;
END;
$$;


ALTER FUNCTION transactions.func_get_commission_rate() OWNER TO postgres;

--
-- TOC entry 264 (class 1255 OID 16566)
-- Name: func_get_user_by_email(text); Type: FUNCTION; Schema: transactions; Owner: postgres
--

CREATE FUNCTION transactions.func_get_user_by_email(p_email text) RETURNS TABLE(userid integer, username character varying, usersurname character varying, useremail text, creationdate timestamp without time zone, lastlogindate timestamp without time zone, pwdhash text)
    LANGUAGE plpgsql
    AS $$
BEGIN
	RETURN query
		SELECT
			u.id AS UserId,
			u.name AS UserName,
			u.surname as UserSurname,
			u.email as UserEmail,
			u.creation_date as CreationDate,
			u.last_login_date as LastLoginDate,
			u.pwd_hash as PwdHash
		FROM
			transactions.users u
		WHERE
			u.email = p_email;
END;
$$;


ALTER FUNCTION transactions.func_get_user_by_email(p_email text) OWNER TO postgres;

--
-- TOC entry 266 (class 1255 OID 16575)
-- Name: func_search_transactions(integer, timestamp without time zone, timestamp without time zone, integer); Type: FUNCTION; Schema: transactions; Owner: postgres
--

CREATE FUNCTION transactions.func_search_transactions(p_user_id integer, p_from timestamp without time zone, p_to timestamp without time zone, p_srcaccid integer) RETURNS TABLE(transactionid integer, originaccid integer, origincurrdescrip character varying, destaccid integer, destcurrdescrip character varying, transactionamount real, transactiondate timestamp without time zone, transactiondescrip character varying)
    LANGUAGE plpgsql
    AS $$
DECLARE

BEGIN
	RETURN query
		SELECT 
			t.id as TransactionId,
			t.origin_acc_id as OriginAccId,
			t.origin_currency_code as OriginCurrCode,
			t.dest_acc_id as DestAccId,
			t.dest_currency_code as DestCurrCode,
			t.amount as TransactionAmount,
			t.date as TransactionDate,
			t.description as TransactionDescrip			
		FROM
			transactions.transfers t
		INNER JOIN transactions.accounts accFrom
			ON accFrom.id = t.origin_acc_id
		WHERE
			accFrom.user_id = p_user_id AND
			(p_from is null or t.date >= p_from) AND
			(p_to is null or t.date < p_to) AND
			(p_srcAccId is null or t.origin_acc_id = p_srcAccId)
		ORDER BY date ASC;			
END;
$$;


ALTER FUNCTION transactions.func_search_transactions(p_user_id integer, p_from timestamp without time zone, p_to timestamp without time zone, p_srcaccid integer) OWNER TO postgres;

--
-- TOC entry 267 (class 1255 OID 16574)
-- Name: func_transfer_amount(integer, character varying, integer, character varying, real, real, timestamp without time zone, character varying, real); Type: FUNCTION; Schema: transactions; Owner: postgres
--

CREATE FUNCTION transactions.func_transfer_amount(p_acc_from integer, p_origin_curr_code character varying, p_acc_to integer, p_dest_curr_code character varying, p_amount_to_debit_on_origin real, p_amount_to_add_on_dest real, p_date timestamp without time zone, p_descrip character varying, p_commission_amount real) RETURNS TABLE(transactionid integer, amountdebited real, commissiondebited real, amounttransferred real)
    LANGUAGE plpgsql
    AS $$
DECLARE	
    v_transaction_id integer;
BEGIN	
	UPDATE transactions.accounts
	SET balance = balance + p_amount_to_add_on_dest
	WHERE id = p_acc_to;
	
	UPDATE transactions.accounts
	SET balance = balance - p_amount_to_debit_on_origin - (p_commission_amount)
	WHERE id = p_acc_from;
	
	INSERT INTO transactions.transfers
	(origin_acc_id, origin_currency_code, dest_acc_id, dest_currency_code, amount, date, description)
	VALUES
	(
		p_acc_from,
		p_origin_curr_code,
		p_acc_to,
		p_dest_curr_code,
		p_amount_to_debit_on_origin,
		p_date,
		p_descrip
	)
	RETURNING id INTO v_transaction_id;
	
	RETURN query
		SELECT 
			v_transaction_id AS TransactionId,
			p_amount_to_debit_on_origin AS AmountDebited,
			p_commission_amount AS CommissionDebited,
			p_amount_to_add_on_dest AS AmountTransferred;	
END;
$$;


ALTER FUNCTION transactions.func_transfer_amount(p_acc_from integer, p_origin_curr_code character varying, p_acc_to integer, p_dest_curr_code character varying, p_amount_to_debit_on_origin real, p_amount_to_add_on_dest real, p_date timestamp without time zone, p_descrip character varying, p_commission_amount real) OWNER TO postgres;

--
-- TOC entry 263 (class 1255 OID 16553)
-- Name: proc_login(text, timestamp without time zone); Type: PROCEDURE; Schema: transactions; Owner: postgres
--

CREATE PROCEDURE transactions.proc_login(IN p_email text, IN p_login_date timestamp without time zone)
    LANGUAGE plpgsql
    AS $$
BEGIN
	UPDATE transactions.users
	SET last_login_date = p_login_date
	WHERE email = p_email;
END;
$$;


ALTER PROCEDURE transactions.proc_login(IN p_email text, IN p_login_date timestamp without time zone) OWNER TO postgres;

--
-- TOC entry 262 (class 1255 OID 16567)
-- Name: proc_register(character varying, character varying, text, text); Type: PROCEDURE; Schema: transactions; Owner: postgres
--

CREATE PROCEDURE transactions.proc_register(IN p_name character varying, IN p_surname character varying, IN p_email text, IN p_pwd_hash text)
    LANGUAGE plpgsql
    AS $$
BEGIN
	INSERT INTO transactions.users
	(name, surname, email, pwd_hash, creation_date)
	VALUES
	(
		p_name,
		p_surname,
		p_email,
		p_pwd_hash,
		NOW()::timestamp
	);
END;
$$;


ALTER PROCEDURE transactions.proc_register(IN p_name character varying, IN p_surname character varying, IN p_email text, IN p_pwd_hash text) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 217 (class 1259 OID 16402)
-- Name: accounts; Type: TABLE; Schema: transactions; Owner: postgres
--

CREATE TABLE transactions.accounts (
    id integer NOT NULL,
    currency_id integer NOT NULL,
    balance real DEFAULT 0,
    user_id integer,
    currency_code character varying,
    user_full_name character varying,
    last_updt_date timestamp without time zone
);


ALTER TABLE transactions.accounts OWNER TO postgres;

--
-- TOC entry 216 (class 1259 OID 16401)
-- Name: accounts_id_seq; Type: SEQUENCE; Schema: transactions; Owner: postgres
--

ALTER TABLE transactions.accounts ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME transactions.accounts_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 219 (class 1259 OID 16408)
-- Name: currencies; Type: TABLE; Schema: transactions; Owner: postgres
--

CREATE TABLE transactions.currencies (
    id integer NOT NULL,
    code character varying NOT NULL,
    description character varying NOT NULL
);


ALTER TABLE transactions.currencies OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 16407)
-- Name: currencies_id_seq; Type: SEQUENCE; Schema: transactions; Owner: postgres
--

ALTER TABLE transactions.currencies ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME transactions.currencies_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 220 (class 1259 OID 16421)
-- Name: transfers; Type: TABLE; Schema: transactions; Owner: postgres
--

CREATE TABLE transactions.transfers (
    id integer NOT NULL,
    origin_acc_id integer NOT NULL,
    dest_acc_id integer NOT NULL,
    amount real DEFAULT 0,
    date timestamp without time zone,
    description character varying,
    origin_currency_code character varying,
    dest_currency_code character varying
);


ALTER TABLE transactions.transfers OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 16469)
-- Name: transactions_id_seq; Type: SEQUENCE; Schema: transactions; Owner: postgres
--

ALTER TABLE transactions.transfers ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME transactions.transactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 222 (class 1259 OID 16442)
-- Name: users; Type: TABLE; Schema: transactions; Owner: postgres
--

CREATE TABLE transactions.users (
    id integer NOT NULL,
    name character varying,
    surname character varying,
    email text NOT NULL,
    pwd_hash text NOT NULL,
    creation_date timestamp without time zone NOT NULL,
    last_login_date timestamp without time zone
);


ALTER TABLE transactions.users OWNER TO postgres;

--
-- TOC entry 221 (class 1259 OID 16441)
-- Name: users_id_seq; Type: SEQUENCE; Schema: transactions; Owner: postgres
--

ALTER TABLE transactions.users ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME transactions.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- TOC entry 3411 (class 0 OID 16402)
-- Dependencies: 217
-- Data for Name: accounts; Type: TABLE DATA; Schema: transactions; Owner: postgres
--

INSERT INTO transactions.accounts (id, currency_id, balance, user_id, currency_code, user_full_name, last_updt_date) OVERRIDING SYSTEM VALUE VALUES (9, 1, 1000, 7, 'UYU', 'John Doe', '2023-04-21 04:36:44.127545');
INSERT INTO transactions.accounts (id, currency_id, balance, user_id, currency_code, user_full_name, last_updt_date) OVERRIDING SYSTEM VALUE VALUES (10, 2, 5000, 7, 'USD', 'John Doe', '2023-04-21 04:37:47.597431');
INSERT INTO transactions.accounts (id, currency_id, balance, user_id, currency_code, user_full_name, last_updt_date) OVERRIDING SYSTEM VALUE VALUES (13, 3, 25000.5, 9, 'EUR', 'Carl Doe', '2023-04-21 04:39:06.743906');
INSERT INTO transactions.accounts (id, currency_id, balance, user_id, currency_code, user_full_name, last_updt_date) OVERRIDING SYSTEM VALUE VALUES (12, 3, 3661.899, 8, 'EUR', 'Jane Doe', '2023-04-21 04:38:42.6022');
INSERT INTO transactions.accounts (id, currency_id, balance, user_id, currency_code, user_full_name, last_updt_date) OVERRIDING SYSTEM VALUE VALUES (11, 2, 29850, 8, 'USD', 'Jane Doe', '2023-04-21 04:38:06.113986');


--
-- TOC entry 3413 (class 0 OID 16408)
-- Dependencies: 219
-- Data for Name: currencies; Type: TABLE DATA; Schema: transactions; Owner: postgres
--

INSERT INTO transactions.currencies (id, code, description) OVERRIDING SYSTEM VALUE VALUES (1, 'UYU', 'Peso uruguayo');
INSERT INTO transactions.currencies (id, code, description) OVERRIDING SYSTEM VALUE VALUES (2, 'USD', 'Dólar estadounidense');
INSERT INTO transactions.currencies (id, code, description) OVERRIDING SYSTEM VALUE VALUES (3, 'EUR', 'Euro');


--
-- TOC entry 3414 (class 0 OID 16421)
-- Dependencies: 220
-- Data for Name: transfers; Type: TABLE DATA; Schema: transactions; Owner: postgres
--

INSERT INTO transactions.transfers (id, origin_acc_id, dest_acc_id, amount, date, description, origin_currency_code, dest_currency_code) OVERRIDING SYSTEM VALUE VALUES (1, 11, 12, 150, '2023-04-21 05:47:06.552', 'Jane USD a EUR', 'USD', 'EUR');


--
-- TOC entry 3416 (class 0 OID 16442)
-- Dependencies: 222
-- Data for Name: users; Type: TABLE DATA; Schema: transactions; Owner: postgres
--

INSERT INTO transactions.users (id, name, surname, email, pwd_hash, creation_date, last_login_date) OVERRIDING SYSTEM VALUE VALUES (7, 'John', 'Doe', 'johndoe@mail.com', '$2a$11$/H8zmj4p108YGrorX3YxGebpA3JyeTW53zjdqxJ1oiWvd5GwM3khG', '2023-04-21 04:31:02.254995', NULL);
INSERT INTO transactions.users (id, name, surname, email, pwd_hash, creation_date, last_login_date) OVERRIDING SYSTEM VALUE VALUES (9, 'Carl', 'Doe', 'carldoe@mail.com', '$2a$11$dlUI/ROhXA3HaF96SkafRe6.YjyzZ.Way//hA0IEB94tsih3oZ4ka', '2023-04-21 04:31:41.850127', '2023-04-21 04:33:02.013213');
INSERT INTO transactions.users (id, name, surname, email, pwd_hash, creation_date, last_login_date) OVERRIDING SYSTEM VALUE VALUES (10, 'Matt', 'Doe', 'mattdoe@mail.com', '$2a$11$grhQqE86JRvaEjcgyWRqIeV5GXv3n6/dlwDgMRQM.I.PJ4NvPtZtW', '2023-04-21 05:30:56.208149', NULL);
INSERT INTO transactions.users (id, name, surname, email, pwd_hash, creation_date, last_login_date) OVERRIDING SYSTEM VALUE VALUES (8, 'Jane', 'Doe', 'janedoe@mail.com', '$2a$11$uVldxOqQ6O89W3L6rfvN4uC9V94bN0qbbrKZ/Rr3o0mYKAwzdTQwi', '2023-04-21 04:31:28.651055', '2023-04-21 05:43:05.590178');


--
-- TOC entry 3431 (class 0 OID 0)
-- Dependencies: 216
-- Name: accounts_id_seq; Type: SEQUENCE SET; Schema: transactions; Owner: postgres
--

SELECT pg_catalog.setval('transactions.accounts_id_seq', 14, true);


--
-- TOC entry 3432 (class 0 OID 0)
-- Dependencies: 218
-- Name: currencies_id_seq; Type: SEQUENCE SET; Schema: transactions; Owner: postgres
--

SELECT pg_catalog.setval('transactions.currencies_id_seq', 3, true);


--
-- TOC entry 3433 (class 0 OID 0)
-- Dependencies: 223
-- Name: transactions_id_seq; Type: SEQUENCE SET; Schema: transactions; Owner: postgres
--

SELECT pg_catalog.setval('transactions.transactions_id_seq', 1, true);


--
-- TOC entry 3434 (class 0 OID 0)
-- Dependencies: 221
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: transactions; Owner: postgres
--

SELECT pg_catalog.setval('transactions.users_id_seq', 10, true);


--
-- TOC entry 3242 (class 2606 OID 16406)
-- Name: accounts accounts_pkey; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.accounts
    ADD CONSTRAINT accounts_pkey PRIMARY KEY (id);


--
-- TOC entry 3245 (class 2606 OID 16440)
-- Name: accounts accounts_unique_key; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.accounts
    ADD CONSTRAINT accounts_unique_key UNIQUE (id) INCLUDE (currency_id);


--
-- TOC entry 3248 (class 2606 OID 16458)
-- Name: accounts curr_user_unique_key; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.accounts
    ADD CONSTRAINT curr_user_unique_key UNIQUE (user_id, currency_id);


--
-- TOC entry 3252 (class 2606 OID 16414)
-- Name: currencies currencies_pkey; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.currencies
    ADD CONSTRAINT currencies_pkey PRIMARY KEY (id);


--
-- TOC entry 3259 (class 2606 OID 16546)
-- Name: users email_unique_pkey; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.users
    ADD CONSTRAINT email_unique_pkey UNIQUE (email);


--
-- TOC entry 3435 (class 0 OID 0)
-- Dependencies: 3259
-- Name: CONSTRAINT email_unique_pkey ON users; Type: COMMENT; Schema: transactions; Owner: postgres
--

COMMENT ON CONSTRAINT email_unique_pkey ON transactions.users IS 'An email can be registered only once';


--
-- TOC entry 3257 (class 2606 OID 16427)
-- Name: transfers transactions_pkey; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.transfers
    ADD CONSTRAINT transactions_pkey PRIMARY KEY (id);


--
-- TOC entry 3261 (class 2606 OID 16448)
-- Name: users user_pkey; Type: CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.users
    ADD CONSTRAINT user_pkey PRIMARY KEY (id);


--
-- TOC entry 3240 (class 1259 OID 16555)
-- Name: accounts_curr_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX accounts_curr_idx ON transactions.accounts USING btree (currency_id);


--
-- TOC entry 3243 (class 1259 OID 16554)
-- Name: accounts_pkey_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX accounts_pkey_idx ON transactions.accounts USING btree (id);


--
-- TOC entry 3246 (class 1259 OID 16556)
-- Name: accounts_user_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX accounts_user_idx ON transactions.accounts USING btree (user_id);


--
-- TOC entry 3249 (class 1259 OID 16564)
-- Name: curr_id_code_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX curr_id_code_idx ON transactions.currencies USING btree (id, code varchar_pattern_ops);


--
-- TOC entry 3250 (class 1259 OID 16563)
-- Name: curr_pkey_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX curr_pkey_idx ON transactions.currencies USING btree (id);


--
-- TOC entry 3253 (class 1259 OID 16558)
-- Name: transac_date_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX transac_date_idx ON transactions.transfers USING btree (date);


--
-- TOC entry 3254 (class 1259 OID 16559)
-- Name: transac_origin_acc_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX transac_origin_acc_idx ON transactions.transfers USING btree (origin_acc_id);


--
-- TOC entry 3255 (class 1259 OID 16557)
-- Name: transac_pkey_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX transac_pkey_idx ON transactions.transfers USING btree (id);


--
-- TOC entry 3262 (class 1259 OID 16562)
-- Name: users_email_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX users_email_idx ON transactions.users USING btree (email text_pattern_ops);


--
-- TOC entry 3263 (class 1259 OID 16561)
-- Name: users_pkey_idx; Type: INDEX; Schema: transactions; Owner: postgres
--

CREATE INDEX users_pkey_idx ON transactions.users USING btree (id);


--
-- TOC entry 3264 (class 2606 OID 16416)
-- Name: accounts currency_id_fkey; Type: FK CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.accounts
    ADD CONSTRAINT currency_id_fkey FOREIGN KEY (currency_id) REFERENCES transactions.currencies(id) NOT VALID;


--
-- TOC entry 3266 (class 2606 OID 16434)
-- Name: transfers dest_acc_fkey; Type: FK CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.transfers
    ADD CONSTRAINT dest_acc_fkey FOREIGN KEY (dest_acc_id) REFERENCES transactions.accounts(id) NOT VALID;


--
-- TOC entry 3267 (class 2606 OID 16428)
-- Name: transfers origin_acc_fkey; Type: FK CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.transfers
    ADD CONSTRAINT origin_acc_fkey FOREIGN KEY (origin_acc_id) REFERENCES transactions.accounts(id);


--
-- TOC entry 3265 (class 2606 OID 16449)
-- Name: accounts user_id_fkey; Type: FK CONSTRAINT; Schema: transactions; Owner: postgres
--

ALTER TABLE ONLY transactions.accounts
    ADD CONSTRAINT user_id_fkey FOREIGN KEY (user_id) REFERENCES transactions.users(id) NOT VALID;


--
-- TOC entry 3424 (class 0 OID 0)
-- Dependencies: 3423
-- Name: DATABASE testdb; Type: ACL; Schema: -; Owner: postgres
--

GRANT ALL ON DATABASE testdb TO test_user;


--
-- TOC entry 3425 (class 0 OID 0)
-- Dependencies: 7
-- Name: SCHEMA transactions; Type: ACL; Schema: -; Owner: test_user
--

REVOKE ALL ON SCHEMA transactions FROM test_user;
GRANT CREATE ON SCHEMA transactions TO test_user;
GRANT USAGE ON SCHEMA transactions TO test_user WITH GRANT OPTION;


--
-- TOC entry 3427 (class 0 OID 0)
-- Dependencies: 217
-- Name: TABLE accounts; Type: ACL; Schema: transactions; Owner: postgres
--

GRANT SELECT,INSERT,REFERENCES,DELETE,UPDATE ON TABLE transactions.accounts TO test_user;


--
-- TOC entry 3428 (class 0 OID 0)
-- Dependencies: 219
-- Name: TABLE currencies; Type: ACL; Schema: transactions; Owner: postgres
--

GRANT SELECT,INSERT,REFERENCES,DELETE,UPDATE ON TABLE transactions.currencies TO test_user;


--
-- TOC entry 3429 (class 0 OID 0)
-- Dependencies: 220
-- Name: TABLE transfers; Type: ACL; Schema: transactions; Owner: postgres
--

GRANT SELECT,INSERT,REFERENCES,DELETE,UPDATE ON TABLE transactions.transfers TO test_user;


--
-- TOC entry 3430 (class 0 OID 0)
-- Dependencies: 222
-- Name: TABLE users; Type: ACL; Schema: transactions; Owner: postgres
--

GRANT SELECT,INSERT,REFERENCES,DELETE,UPDATE ON TABLE transactions.users TO test_user;


-- Completed on 2023-04-21 05:48:24

--
-- PostgreSQL database dump complete
--

